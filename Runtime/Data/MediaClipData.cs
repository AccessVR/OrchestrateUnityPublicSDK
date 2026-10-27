using System;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// The part of a video or audio file one Layer plays: from its in point to
    /// its out point, in seconds of the file. The file is never cut, so the
    /// player seeks to the in point and stops at the out point. An absent out
    /// point plays to the end of the file.
    ///
    /// A Layer's own time runs from 0; the file's runs from the in point. The
    /// server and the web player read the same shape (MediaClip.php,
    /// mediaClip.js).
    /// </summary>
    [Serializable]
    public class MediaClipData
    {
        /// <summary>How close to the out point counts as having reached it.</summary>
        public const double OutPointToleranceSeconds = 0.05;

        [JsonProperty("in")] private double? _in;
        [JsonProperty("out")] private double? _out;

        public static readonly MediaClipData Whole = new();

        public MediaClipData()
        {
        }

        public MediaClipData(double inPoint, double? outPoint)
        {
            _in = inPoint;
            _out = outPoint;
        }

        /// <summary>Where the clip starts in the file; never before its start.</summary>
        [JsonIgnore] public double In => Math.Max(0.0, _in ?? 0.0);

        /// <summary>Where the clip stops in the file, or null to play to the end.</summary>
        [JsonIgnore] public double? Out => _out.HasValue && _out.Value > In ? _out : null;

        [JsonIgnore] public bool IsTrimmed => In > 0.0 || Out.HasValue;

        /// <summary>The file's time at a moment of the Layer's own time.</summary>
        public double FileTimeAt(double layerTime) => In + Math.Max(0.0, layerTime);

        /// <summary>The Layer's own time at a moment of the file.</summary>
        public double LayerTimeAt(double fileTime) => fileTime - In;

        /// <summary>Where the clip stops: its out point, or the file's end.</summary>
        public double End(double fileDuration) => Out ?? fileDuration;

        /// <summary>How long the clip plays, given the file's length.</summary>
        public double Length(double fileDuration) => Math.Max(0.0, End(fileDuration) - In);

        public bool HasReachedOutPoint(double fileTime) =>
            Out.HasValue && fileTime >= Out.Value - OutPointToleranceSeconds;
    }
}
