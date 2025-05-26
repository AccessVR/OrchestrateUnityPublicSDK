using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AccessVR.OrchestrateVR.SDK
{

    public enum Positioning
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum CardSizeOptions
    {
        Small,
        Medium,
        Large
    }

    public enum DisplayTypeOptions
    {
        HUD,
        WorldSpace,
        Hidden,
    }

    [Serializable]
    public abstract class AbstractEventData : AbstractData
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("startTime")] public double StartTime = 0.0;
        [JsonProperty("endTime")] public double EndTime = 0.0;
        [JsonProperty("positioning")] public int? PositionIndex = null;
        [JsonIgnore] public Positioning Position = Positioning.BottomRight;
        [JsonProperty("pausePlayback")] public bool PausePlayback = false;
        [JsonProperty("canBeSkipped")] public bool Skippable = false;
        [JsonProperty("displayType")] public int? DisplayTypeIndex = null;
        [JsonIgnore] public DisplayTypeOptions DisplayType = DisplayTypeOptions.HUD;
        [JsonProperty("cardSize")] public int? CardSizeIndex = null;
        [JsonIgnore] public CardSizeOptions CardSize = CardSizeOptions.Medium;
        [JsonProperty("text")] public string Text = "";
        [JsonProperty("title")] public string Title = "";
        [JsonProperty("name")] public string Name = "";
        [JsonProperty("description")] public string Description = "";
        [JsonProperty("unity_description")] public JObject UnityDescriptionJson;
        [JsonProperty("buttonColor")] public string ButtonColorString;
        [JsonProperty("buttonLabelColor")] public string ButtonLabelColorString;
        [JsonProperty("backgroundColor")] public string BackgroundColorString;
        [JsonIgnore] public Color? ButtonColor;
        [JsonIgnore] public Color? ButtonLabelColor;
        [JsonIgnore] public Color? BackgroundColor;
        [JsonProperty("asset")] public JObject AssetJson;
        [JsonIgnore] public AssetData Asset;
        private IEventView _view;
        public IEventView View => CreateView();
        public UnityEvent doOnRemove = new UnityEvent();
        public delegate void OnAcknowledgedDelegate(AbstractEventData eventData);
        public OnAcknowledgedDelegate OnAcknowledged;
        [FormerlySerializedAs("isSubAction")] public bool isActionEvent = false;
        private bool acknowledged;
        public SceneData parentScene;

        protected AbstractEventData() { }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Handle Position
            if (PositionIndex.HasValue)
            {
                Position = PositionIndex.Value switch
                {
                    0 => Positioning.TopLeft,
                    1 => Positioning.TopCenter,
                    2 => Positioning.TopRight,
                    3 => Positioning.MiddleLeft,
                    4 => Positioning.MiddleCenter,
                    5 => Positioning.MiddleRight,
                    6 => Positioning.BottomLeft,
                    7 => Positioning.BottomCenter,
                    8 => Positioning.BottomRight,
                    _ => Positioning.BottomRight
                };
            }
            // Handle CardSize
            if (CardSizeIndex.HasValue)
            {
                CardSize = CardSizeIndex.Value switch
                {
                    0 => CardSizeOptions.Small,
                    1 => CardSizeOptions.Medium,
                    2 => CardSizeOptions.Large,
                    _ => CardSizeOptions.Medium
                };
            }
            // Handle DisplayType
            if (DisplayTypeIndex.HasValue)
            {
                DisplayType = DisplayTypeIndex.Value switch
                {
                    1 => DisplayTypeOptions.WorldSpace,
                    2 => DisplayTypeOptions.Hidden,
                    _ => DisplayTypeOptions.HUD
                };
            }
            // Handle UnityRichText for description
            if (UnityDescriptionJson != null)
            {
                UnityRichText description = new UnityRichText(UnityDescriptionJson);
                if (!string.IsNullOrEmpty(description.Content))
                {
                    Text = Description = description.Content;
                }
            }
            // Handle Color fields
            ButtonColor = ConvertToColor(ButtonColorString);
            ButtonLabelColor = ConvertToColor(ButtonLabelColorString);
            BackgroundColor = ConvertToColor(BackgroundColorString);
            // Handle Asset
            if (AssetJson != null && AssetJson.ToString().Length > 0)
            {
                Asset = AssetJson.ToObject<AssetData>();
            }
            // Validation logic
            if (EndTime < StartTime)
                PausePlayback = true;
        }

        protected Color? ConvertToColor(string color)
        {
            if (string.IsNullOrEmpty(color))
                return null;
            if (color.StartsWith("#"))
            {
                ColorUtility.TryParseHtmlString(color, out Color parsedColor);
                return parsedColor;
            }
            else if (color.StartsWith("rgb"))
            {
                string[] values = color.Replace("rgba(", "").Replace("rgb(", "").Replace(")", "").Split(',');
                if (values.Length >= 3)
                {
                    float r = float.Parse(values[0]) / 255f;
                    float g = float.Parse(values[1]) / 255f;
                    float b = float.Parse(values[2]) / 255f;
                    float a = values.Length > 3 ? float.Parse(values[3]) : 1f;
                    return new Color(r, g, b, a);
                }
            }
            return null;
        }

        public IEventView CreateView()
        {
            if (_view == null)
            {
                _view = EventViewFactory.CreateView(this);
                _view.Hide();
            }
            return _view;
        }

        public void DestroyView()
        {
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
        }
        
        public bool HasBeenAcknowledged() => acknowledged;
        public void Acknowledge()
        {
            acknowledged = true;
            OnAcknowledged?.Invoke(this);
        }
        public void SetHasBeenAcknowledged(bool acknowledged) => this.acknowledged = acknowledged;
        public void Unacknowledge() => acknowledged = false;
        public virtual bool ShouldPauseForAcknowledgement() => PausesPlayback();
        public bool PausesPlayback() => PausePlayback;
    }
}
