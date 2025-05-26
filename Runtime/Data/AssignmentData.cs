using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System;

namespace AccessVR.OrchestrateVR.SDK
{

    public class AssignmentData : AbstractData
    {
        [JsonProperty("createdBy")] public string createdBy;
        [JsonProperty("course")] public CourseData course;
        [JsonProperty("dueDate")] public DateTime dueDate;
        [JsonProperty("createDate")] public DateTime createdDate;
        [JsonProperty("guid")] public string guid;
        [JsonProperty("isCompleted")] public bool isCompleted = false;
        [JsonProperty("completedDate")] public DateTime completedDate;
        public string DateFormatString = "M/d/yyyy h:mm:ss tt";
        
        private bool isValid = false;
        public bool IsValid { get { return isValid; } }
        private bool isLocal = false;
        public bool IsLocal { get { return isLocal; } }



        public AssignmentData(JObject jsonObject)
        {
            LoadFromJson(jsonObject);
        }

        public AssignmentData(string guid)
        {
            if (Directory.Exists(Path.Combine(Application.persistentDataPath, guid)))
            {
                string filePath = Path.Combine(Application.persistentDataPath, guid + "/assignment.json");
                JObject storedJson;
                if (!File.Exists(filePath))
                {
                    Debug.Log("Could not find local json: " + filePath);
                    return;
                }
                try
                {
                    storedJson = JObject.Parse(File.ReadAllText(filePath));
                    //IsSummary = false;
                    isLocal = true;
                    LoadFromJson(storedJson);

                }
                catch (JsonReaderException ex)
                {
                    Debug.Log("Error reading local json: " + ex.Message);
                }
            }
        }

        public void LoadFromJson(JObject jsonObject)
        {
            if (jsonObject["id"] != null)
            {
                guid = jsonObject["id"].ToString();
            }

            if (jsonObject["createdBy"] != null)
            {
                createdBy = jsonObject["id"].ToString();
            }

            //Grab Date Data
            if (jsonObject["createDate"] != null)
            {
                string dateString = jsonObject["createDate"].ToString();
                DateTime.TryParseExact(dateString, DateFormatString, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces |
                                System.Globalization.DateTimeStyles.AdjustToUniversal,
                                out createdDate);
            }
            if (jsonObject["dueDate"] != null)
            {
                string dateString = jsonObject["dueDate"].ToString();
                DateTime.TryParseExact(dateString, DateFormatString, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces |
                                System.Globalization.DateTimeStyles.AdjustToUniversal,
                                out dueDate);
            }

            //Grab Course Data
            if (jsonObject["course"] != null)
            {
                JObject courseObject = jsonObject["course"].ToObject<JObject>();
                course = new CourseData(courseObject);
            }


            //sets this data to be marked as valid, aka downloaded correctly
            isValid = true;


            //saves the downloaded json data locally
            //SaveAssignmentJsonLocally(jsonObject.ToString());
        }
        

        //Saves the Assignment's json data, not the assets needed to play lessons or the thumbnails, etc
        //those are handled by their approprieate data script
        private void SaveAssignmentJsonLocally(string assignmentJson)
        {
            isLocal = true;
            File.WriteAllText(Path.Combine(Application.persistentDataPath, guid + "/assignment.json"), assignmentJson);
        }

    }

}

