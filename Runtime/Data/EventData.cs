using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

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
        Hotspots,
    }

    /// <remarks>
    /// The editor also writes authoring-only keys onto event JSON that this
    /// class deliberately does not map: <c>locked</c> (stage-edit freeze; the
    /// layer plays normally) and <c>_worldspaceInitialized</c> (editor seed
    /// sentinel). Json.NET ignores unknown members, so they ride through
    /// harmlessly.
    /// </remarks>
    [Serializable]
    public abstract class EventData : Data, IDownloadable
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("startTime")] public double StartTime = 0.0;
        [JsonProperty("endTime")] public double EndTime = 0.0;
        [JsonProperty("pausePlayback")] private bool _pausePlayback = false;
        [JsonProperty("canBeSkipped")] public bool Skippable = false;
        [JsonProperty("name")] public string Name = "";
        [JsonProperty("title")] public string Title = "";
        [JsonProperty("description")] public string Description = "";
        [JsonProperty("muteAudio")] public bool MuteAudio;
        
        [JsonProperty("unity_description")] private UnityRichText UnityDescription;
        [JsonProperty("text")] private string Text = "";
        
        [JsonProperty("buttonColor")] private string _buttonColor;
        [JsonProperty("buttonLabelColor")] private string _buttonLabelColor;
        [JsonProperty("buttonText")] public string ButtonText = "OK";
        
        [JsonProperty("backgroundColor")] private string _backgroundColor;
        
        [JsonProperty("asset")] public AssetData Asset;
        
        [JsonProperty("displayType")] private int? _displayType = null;
        [JsonProperty("positioning")] private int? _position = null;
        [JsonProperty("cardSize")] private int? _cardSize = null;

        /// <summary>
        /// Worldspace position relative to the camera, in Three.js right-handed
        /// coordinates. Nullable: an absent value means "no position set".
        /// The editor writes these transform fields on any worldspace-capable
        /// event (Media, Text/Info, Question), which is why they live here on
        /// the base class rather than on <see cref="MediaEventData"/>.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public Vector3? Position;

        /// <summary>
        /// Worldspace rotation in Euler degrees. Only honored when
        /// <see cref="SpriteMode"/> is false and <see cref="DisplayType"/>
        /// is <see cref="DisplayTypeOptions.WorldSpace"/>; otherwise the
        /// renderer billboards the layer toward the camera.
        /// </summary>
        [JsonProperty("rotation")] public Vector3 Rotation;

        /// <summary>
        /// When true (default), a Worldspace layer always faces the camera
        /// (billboard). When false, the layer holds the authored
        /// <see cref="Rotation"/>. Default true is load-bearing: a fresh
        /// switch-to-Worldspace behaves like a Hotspot without the author
        /// having to choose an orientation.
        /// </summary>
        [JsonProperty("spriteMode")] public bool SpriteMode = true;

        /// <summary>
        /// Per-axis scale multiplier set by the editor's scale-mode gizmo.
        /// Nullable: absent means "the author hasn't scaled this layer";
        /// treated as {1,1,1} when null.
        /// </summary>
        [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
        public Vector3? Scale;

        /// <summary>
        /// "Glass Blur" behind HUD media cards (web default true). Deserialized
        /// for contract completeness but NOT rendered: no backdrop-blur stack
        /// exists in this project, and a stereo grab-pass on Quest is outside
        /// the current render budget. HUD cards keep their opaque/tinted
        /// background instead.
        /// </summary>
        [JsonProperty("backdropBlur")] public bool BackdropBlur = true;
        
        [JsonIgnore] public CardSizeOptions CardSize = CardSizeOptions.Medium;
        [JsonIgnore] public Positioning HudPositioning = Positioning.BottomRight;
        [JsonIgnore] public DisplayTypeOptions DisplayType = DisplayTypeOptions.HUD;
        [JsonIgnore] public Color? ButtonColor;
        [JsonIgnore] public Color? ButtonLabelColor;
        [JsonIgnore] public Color? BackgroundColor;

        private IEventView _view;
        public IEventView View => CreateView();
        public UnityEvent doOnRemove = new UnityEvent();
        public delegate void OnAcknowledgedDelegate(EventData eventData);
        public OnAcknowledgedDelegate OnAcknowledged;

        private bool _isActionEvent;
        
        private bool acknowledged;

        protected EventData() { }

        public virtual void AfterDeserialized(StreamingContext context)
        {
            //
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            if (_position.HasValue)
            {
                HudPositioning = _position.Value switch
                {
                    0 => Positioning.TopLeft,
                    1 => Positioning.TopCenter,
                    2 => Positioning.TopRight,
                    3 => Positioning.MiddleLeft,
                    4 => Positioning.MiddleCenter,
                    5 => Positioning.MiddleRight,
                    6 => Positioning.BottomLeft,
                    7 => Positioning.BottomCenter,
                    _ => Positioning.BottomRight
                };
            }

            if (_cardSize.HasValue)
            {
                CardSize = _cardSize.Value switch
                {
                    0 => CardSizeOptions.Small,
                    1 => CardSizeOptions.Medium,
                    2 => CardSizeOptions.Large,
                    _ => CardSizeOptions.Medium
                };
            }

            if (_displayType.HasValue)
            {
                DisplayType = _displayType.Value switch
                {
                    1 => DisplayTypeOptions.WorldSpace,
                    2 => DisplayTypeOptions.Hidden,
                    3 => DisplayTypeOptions.Hotspots,
                    _ => DisplayTypeOptions.HUD
                };
            }
            
            // legacy support for "Text" property
            if (string.IsNullOrEmpty(Description))
            {
                Description = Text;
            }
            // support for Unity Rich Text formatting
            if (!string.IsNullOrEmpty(UnityDescription?.Content))
            {
                Description = UnityDescription.Content;
            }
            
            ButtonColor = StringUtils.ConvertToColor(_buttonColor);
            ButtonLabelColor = StringUtils.ConvertToColor(_buttonLabelColor);
            BackgroundColor = StringUtils.ConvertToColor(_backgroundColor);

            if (EndTime < StartTime)
            {
                _pausePlayback = true;
            }
        }

        public bool HasAsset()
        {
            return Asset != null;
        }

        public void SetIsActionEvent(bool isActionEvent)
        {
            _isActionEvent = isActionEvent;
        }

        public bool IsActionEvent()
        {
            return _isActionEvent;
        }

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Asset?.SetParentScene(scene);
        }

        public virtual List<DownloadableFileData> GetDownloadableFiles()
        {
            List<DownloadableFileData> list = new();
            
            list.AddRange(Asset?.GetDownloadableFiles() ?? new());
            
            return list.Where(file => file != null).ToList();
        }

        public IEventView CreateView()
        {
            if (_view == null || _view.IsDestroyed())
            {
                _view = EventViewFactory.Make(this);
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
        public bool PausesPlayback() => _pausePlayback;
    }
}
