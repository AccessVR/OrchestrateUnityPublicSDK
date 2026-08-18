using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// Append-only JSONL journal for records that must survive offline
    /// periods and app restarts — analytics events captured with no
    /// connectivity, submissions for runs the server hasn't heard about.
    /// Plain files on disk (the SessionManager per-environment file pattern),
    /// deliberately NOT PlayerPrefs and NOT any in-memory queue: a lost run
    /// is a lost training record.
    ///
    /// One line per record. Corrupted lines (a torn write from a crash
    /// mid-append) are skipped on read and dropped on the next truncation.
    /// Bounded: when the journal exceeds <see cref="MaxEntries"/>, the oldest
    /// entries are dropped with a logged warning — the queue must never grow
    /// without limit on a headset that stays offline for a month.
    /// </summary>
    public class OXRDurableQueue
    {
        public const int DefaultMaxEntries = 5000;

        private readonly string _path;
        private readonly int _maxEntries;
        private readonly object _lock = new();

        public OXRDurableQueue(string path, int maxEntries = DefaultMaxEntries)
        {
            _path = StringUtils.AssertNotNullOrEmpty(path);
            _maxEntries = maxEntries;
        }

        /// <summary>
        /// The conventional per-environment queue path, e.g.
        /// {persistentDataPath}/{env}.abxr-queue.jsonl.
        /// </summary>
        public static string PathFor(string environmentName, string queueName)
        {
            return Path.Combine(
                Application.persistentDataPath,
                environmentName.ToLower() + "." + queueName + ".jsonl");
        }

        public string FilePath => _path;

        /// <summary>
        /// Appends one record as a single JSON line, flushed to disk before
        /// returning. Callers serialize; the queue only guards the line
        /// discipline (no raw newlines inside a record).
        /// </summary>
        public void Append(string jsonLine)
        {
            if (string.IsNullOrEmpty(jsonLine))
            {
                return;
            }
            if (jsonLine.Contains('\n'))
            {
                jsonLine = jsonLine.Replace("\r", "").Replace('\n', ' ');
            }

            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");

                // A crash mid-append can leave a torn line with no trailing
                // newline; appending straight after it would glue this record
                // onto the corrupted tail and destroy both. Heal the tail
                // first so the torn line stays alone on its own line.
                if (FileEndsWithoutNewline())
                {
                    File.AppendAllText(_path, "\n", Encoding.UTF8);
                }

                using StreamWriter writer = new StreamWriter(_path, append: true, Encoding.UTF8);
                writer.WriteLine(jsonLine);
                writer.Flush();
            }

            EnforceBounds();
        }

        /// <summary>
        /// All well-formed records, oldest first. Lines that do not parse as
        /// JSON objects are skipped: a torn tail write must not poison the
        /// whole journal.
        /// </summary>
        public List<string> ReadAll()
        {
            lock (_lock)
            {
                if (!File.Exists(_path))
                {
                    return new List<string>();
                }

                List<string> records = new();
                foreach (string line in File.ReadAllLines(_path, Encoding.UTF8))
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0)
                    {
                        continue;
                    }
                    if (IsPlausibleJsonObject(trimmed))
                    {
                        records.Add(trimmed);
                    }
                    else
                    {
                        Debug.LogWarning($"[OXRDurableQueue] skipping corrupted line in {Path.GetFileName(_path)}");
                    }
                }
                return records;
            }
        }

        public int Count() => ReadAll().Count;

        /// <summary>
        /// Removes the first <paramref name="count"/> records — call after a
        /// flush the server acknowledged. Records appended concurrently are
        /// preserved.
        /// </summary>
        public void RemoveFirst(int count)
        {
            if (count <= 0)
            {
                return;
            }
            lock (_lock)
            {
                List<string> remaining = ReadAllUnlocked();
                if (count >= remaining.Count)
                {
                    File.Delete(_path);
                    return;
                }
                remaining.RemoveRange(0, count);
                RewriteUnlocked(remaining);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
            }
        }

        private void EnforceBounds()
        {
            lock (_lock)
            {
                List<string> records = ReadAllUnlocked();
                if (records.Count <= _maxEntries)
                {
                    return;
                }
                int dropped = records.Count - _maxEntries;
                Debug.LogWarning($"[OXRDurableQueue] {Path.GetFileName(_path)} over capacity; dropping {dropped} oldest record(s)");
                records.RemoveRange(0, dropped);
                RewriteUnlocked(records);
            }
        }

        private bool FileEndsWithoutNewline()
        {
            if (!File.Exists(_path))
            {
                return false;
            }
            using FileStream stream = File.OpenRead(_path);
            if (stream.Length == 0)
            {
                return false;
            }
            stream.Seek(-1, SeekOrigin.End);
            return stream.ReadByte() != '\n';
        }

        private List<string> ReadAllUnlocked()
        {
            if (!File.Exists(_path))
            {
                return new List<string>();
            }
            List<string> records = new();
            foreach (string line in File.ReadAllLines(_path, Encoding.UTF8))
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 0 && IsPlausibleJsonObject(trimmed))
                {
                    records.Add(trimmed);
                }
            }
            return records;
        }

        private void RewriteUnlocked(List<string> records)
        {
            string temp = _path + ".tmp";
            File.WriteAllLines(temp, records, Encoding.UTF8);
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            File.Move(temp, _path);
        }

        private static bool IsPlausibleJsonObject(string line)
        {
            if (!line.StartsWith("{") || !line.EndsWith("}"))
            {
                return false;
            }
            try
            {
                Newtonsoft.Json.Linq.JObject.Parse(line);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
