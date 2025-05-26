using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
	public enum ColorStyleOptions 
	{
		StandardText,
		Red,
		Yellow,
		Green,
		Orange,
		OrangeHighlight,
		OrangeDarkened,
		Gray,
		GrayTransparent,
		GrayHighlight,
		GrayHighlight2,
		GrayDarkened,
		Transparent
	}

	public enum ButtonStyleSettings 
	{
		Primary,
		Secondary,
		Tirtiary,
		CardButton
	}

	[System.Serializable]
	public class ButtonColorStyle
	{
		public ColorStyleOptions normalColor;
		public ColorStyleOptions highlightedColor;
		public ColorStyleOptions pressedColor;
		public ColorStyleOptions selectedColor;
		public ColorStyleOptions disabledColor;
	}


	[CreateAssetMenu(fileName = "StyleData", menuName = "DataObjects/StyleData", order = 1)]
	public class StyleRefSettings : ScriptableObject
	{
		[SerializeField] private Color _standardTextColor;
		public Color StandardTextColor
		{
			get { return _standardTextColor; }
		}



		[SerializeField] private Color _orchestrateRed;
		public Color OrchestrateRed
		{
			get { return _orchestrateRed; }
		}

		[SerializeField] private Color _orchestrateYellow;
		public Color OrchestrateYellow
		{
			get { return _orchestrateYellow; }
		}

		[SerializeField] private Color _orchestrateGreen;
		public Color OrchestrateGreen
		{
			get { return _orchestrateGreen; }
		}



		[SerializeField] private Color _orchestrateOrange;
		public Color OrchestrateOrange
		{
			get { return _orchestrateOrange; }
		}

		[SerializeField] private Color _orchestrateOrangeHighlight;
		public Color OrchestrateOrangeHighlight
		{
			get { return _orchestrateOrangeHighlight; }
		}

		[SerializeField] private Color _orchestrateOrangeDarkened;
		public Color OrchestrateOrangeDarkened
		{
			get { return _orchestrateOrangeDarkened; }
		}



		[SerializeField] private Color _orchestrateGray;
		public Color OrchestrateGray
		{
			get { return _orchestrateGray; }
		}

		[SerializeField] private Color _orchestrateGrayTransparent;
		public Color OrchestrateGrayTransparent
		{
			get { return _orchestrateGrayTransparent; }
		}

		[SerializeField] private Color _orchestrateGrayHighlight;
		public Color OrchestrateGrayHighlight 
		{
			get { return _orchestrateGrayHighlight; }
		}

		[SerializeField] private Color _orchestrateGrayHighlight2;
		public Color OrchestrateGrayHighlight2
		{
			get { return _orchestrateGrayHighlight2; }
		}

		[SerializeField] private Color _orchestrateGrayDarkened;
		public Color OrchestrateGrayDarkened
		{
			get { return _orchestrateGrayDarkened; }
		}

		[SerializeField] private Color _transparent;
		public Color Transparent
		{
			get { return _transparent; }
		}


		//button settings
		[SerializeField] private ButtonColorStyle _primaryButton;
		public ButtonColorStyle PrimaryButton
		{
			get { return _primaryButton; }
		}


		[SerializeField] private ButtonColorStyle _secondaryButton;
		public ButtonColorStyle SecondaryButton
		{
			get { return _secondaryButton; }
		}

		[SerializeField] private ButtonColorStyle _tirtiaryButton;
		public ButtonColorStyle TirtiaryButton
		{
			get { return _tirtiaryButton; }
		}

		[SerializeField] private ButtonColorStyle _cardButton;
		public ButtonColorStyle CardButton
		{
			get { return _cardButton; }
		}
	}

}

