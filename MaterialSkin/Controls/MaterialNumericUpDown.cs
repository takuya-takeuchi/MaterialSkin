using System.Globalization;

namespace MaterialSkin.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;
    using MaterialSkin.Animations;

    public class MaterialNumericUpDown : Control, IMaterialControl
    {

        MaterialContextMenuStrip cms = new BaseTextBoxContextMenuStrip();
        ContextMenuStrip _lastContextMenuStrip = new ContextMenuStrip();

        //Properties for managing the material design properties
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        //Unused properties
        [Browsable(false)]
        public override System.Drawing.Image BackgroundImage { get; set; }

        [Browsable(false)]
        public override System.Windows.Forms.ImageLayout BackgroundImageLayout { get; set; }

        [Browsable(false)]
        public string SelectedText { get { return baseTextBox.SelectedText; } set { baseTextBox.SelectedText = value; } }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor { get; set; }


        //Material Skin properties

        private bool _UseTallSize;

        [Category("Material Skin"), DefaultValue(true), Description("Using a larger size enables the hint to always be visible")]
        public bool UseTallSize
        {
            get { return _UseTallSize; }
            set
            {
                _UseTallSize = value;
                UpdateHeight();
                UpdateRects();
                Invalidate();
            }
        }

        private bool _showAssistiveText;
        [Category("Material Skin"), DefaultValue(false), Description("Assistive elements provide additional detail about text entered into text fields. Could be Helper text or Error message.")]
        public bool ShowAssistiveText
        {
            get { return _showAssistiveText; }
            set
            {
                _showAssistiveText = value;
                if (_showAssistiveText)
                    _helperTextHeight = HELPER_TEXT_HEIGHT;
                else
                    _helperTextHeight = 0;
                UpdateHeight();
                //UpdateRects();
                Invalidate();
            }
        }

        private string _helperText;

        [Category("Material Skin"), DefaultValue(""), Localizable(true), Description("Helper text conveys additional guidance about the input field, such as how it will be used.")]
        public string HelperText
        {
            get { return _helperText; }
            set
            {
                _helperText = value;
                Invalidate();
            }
        }

        private string _errorMessage;

        [Category("Material Skin"), DefaultValue(""), Localizable(true), Description("When text input isn't accepted, an error message can display instructions on how to fix it. Error messages are displayed below the input line, replacing helper text until fixed.")]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                Invalidate();
            }
        }

        [Category("Material Skin"), DefaultValue(true)]
        public bool UseAccent { get; set; }

        private Image _upIcon;

        public Image UpIcon
        {
            get { return _upIcon; }
            set
            {
                _upIcon = value;
                UpdateRects();
                preProcessIcons();
                Invalidate();
            }
        }

        private Image _downIcon;

        public Image DownIcon
        {
            get { return _downIcon; }
            set
            {
                _downIcon = value;
                UpdateRects();
                preProcessIcons();
                Invalidate();
            }
        }

        private decimal _value;

        [System.ComponentModel.Bindable(true)]
        public decimal Value
        {
            get { return _value; }
            set
            {
                _value = value;
                baseTextBox.Text = value.ToString(CultureInfo.InvariantCulture);
                UpdateRects();
                Invalidate();
            }
        }

        private decimal _maximum = 100;

        [System.ComponentModel.Bindable(true)]
        [DefaultValue(100)]
        public decimal Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                Invalidate();
            }
        }

        private decimal _minimum = 0;

        [System.ComponentModel.Bindable(true)]
        [DefaultValue(0)]
        public decimal Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                Invalidate();
            }
        }

        private decimal _increment = 1;

        [System.ComponentModel.Bindable(true)]
        [DefaultValue(1)]
        public decimal Increment
        {
            get { return _increment; }
            set
            {
                _increment = value;
                Invalidate();
            }
        }

        public enum PrefixSuffixTypes
        {
            None,
            Prefix,
            Suffix,
        }

        private PrefixSuffixTypes _prefixsuffix;
        [Category("Material Skin"), DefaultValue(PrefixSuffixTypes.None), Description("Set Prefix/Suffix/None")]
        public PrefixSuffixTypes PrefixSuffix
        {
            get { return _prefixsuffix; }
            set
            {
                _prefixsuffix = value;
                UpdateRects();            //Génére une nullref exception
                if (_prefixsuffix == PrefixSuffixTypes.Suffix)
                    RightToLeft = RightToLeft.Yes;
                else
                    RightToLeft = RightToLeft.No;
                Invalidate();
            }
        }

        private string _prefixsuffixText;
        [Category("Material Skin"), DefaultValue(""), Localizable(true), Description("Set Prefix or Suffix text")]
        public string PrefixSuffixText
        {
            get { return _prefixsuffixText; }
            set
            {
                //if (_prefixsuffixText != value)
                //{
                    _prefixsuffixText = value;
                    UpdateRects();
                    Invalidate();
                //}
            }
        }

        //TextBox properties

        public override ContextMenuStrip ContextMenuStrip
        {
            get { return baseTextBox.ContextMenuStrip; }
            set
            {
                if (value != null)
                {
                    //ContextMenuStrip = value;
                    baseTextBox.ContextMenuStrip = value;
                    base.ContextMenuStrip = value;
                }
                else
                {
                    //ContextMenuStrip = cms;
                    baseTextBox.ContextMenuStrip = cms;
                    base.ContextMenuStrip = cms;
                }
                _lastContextMenuStrip = base.ContextMenuStrip;
            }
        }

        [Browsable(false)]
        public override Color BackColor { get { return Parent == null ? SkinManager.BackgroundColor : Parent.BackColor; } }

        //public override string Text { get { return baseTextBox.Text; } set { baseTextBox.Text = value; UpdateRects(); } }
        public override string Text { get { return null; } set { } }

        [Category("Appearance")]
        public HorizontalAlignment TextAlign { get { return baseTextBox.TextAlign; } set { baseTextBox.TextAlign = value; } }

        public new object Tag { get { return baseTextBox.Tag; } set { baseTextBox.Tag = value; } }

        private bool _readonly;
        [Category("Behavior")]
        public bool ReadOnly
        {
            get { return _readonly; }
            set
            {
                _readonly = value;
                if (Enabled == true)
                {
                    baseTextBox.ReadOnly = _readonly;
                }
                this.Invalidate();
            }
        }

        private bool _animateReadOnly;

        [Category("Material Skin")]
        [Browsable(true)]
        public bool AnimateReadOnly
        {
            get => _animateReadOnly;
            set
            {
                _animateReadOnly = value;
                Invalidate();
            }
        }

        private bool _leaveOnEnterKey;

        [Category("Material Skin"), DefaultValue(false), Description("Select next control which have TabStop property set to True when enter key is pressed.")]
        public bool LeaveOnEnterKey
        {
            get => _leaveOnEnterKey;
            set
            {
                _leaveOnEnterKey = value;
                if (value)
                {
                    baseTextBox.KeyDown += new KeyEventHandler(LeaveOnEnterKey_KeyDown);
                }
                else
                {
                    baseTextBox.KeyDown -= LeaveOnEnterKey_KeyDown;
                }
                Invalidate();
            }
        }

        public void SelectAll() { baseTextBox.SelectAll(); }

        public void Clear() { baseTextBox.Clear(); }

        public void Copy() { baseTextBox.Copy(); }

        public void Cut() { baseTextBox.Cut(); }

        public void Undo() { baseTextBox.Undo(); }

        public void Paste() { baseTextBox.Paste(); }

        #region "Events"

        [Category("Action")]
        [Description("Fires when Leading Icon is clicked")]
        public event EventHandler LeadingIconClick;

        [Category("Action")]
        [Description("Fires when Trailing Icon is clicked")]
        public event EventHandler TrailingIconClick;

        #endregion

        # region Forwarding events to baseTextBox

        public new event EventHandler AutoSizeChanged
        {
            add
            {
                baseTextBox.AutoSizeChanged += value;
            }
            remove
            {
                baseTextBox.AutoSizeChanged -= value;
            }
        }

        public new event EventHandler BackgroundImageChanged
        {
            add
            {
                baseTextBox.BackgroundImageChanged += value;
            }
            remove
            {
                baseTextBox.BackgroundImageChanged -= value;
            }
        }

        public new event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                baseTextBox.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                baseTextBox.BackgroundImageLayoutChanged -= value;
            }
        }

        public new event EventHandler BindingContextChanged
        {
            add
            {
                baseTextBox.BindingContextChanged += value;
            }
            remove
            {
                baseTextBox.BindingContextChanged -= value;
            }
        }

        public new event EventHandler CausesValidationChanged
        {
            add
            {
                baseTextBox.CausesValidationChanged += value;
            }
            remove
            {
                baseTextBox.CausesValidationChanged -= value;
            }
        }

        public new event UICuesEventHandler ChangeUICues
        {
            add
            {
                baseTextBox.ChangeUICues += value;
            }
            remove
            {
                baseTextBox.ChangeUICues -= value;
            }
        }

        public new event EventHandler Click
        {
            add
            {
                baseTextBox.Click += value;
            }
            remove
            {
                baseTextBox.Click -= value;
            }
        }

        public new event EventHandler ClientSizeChanged
        {
            add
            {
                baseTextBox.ClientSizeChanged += value;
            }
            remove
            {
                baseTextBox.ClientSizeChanged -= value;
            }
        }

        #if NETFRAMEWORK
        public new event EventHandler ContextMenuChanged
        {
            add
            {
                baseTextBox.ContextMenuChanged += value;
            }
            remove
            {
                baseTextBox.ContextMenuChanged -= value;
            }
        }
        #endif

        public new event EventHandler ContextMenuStripChanged
        {
            add
            {
                baseTextBox.ContextMenuStripChanged += value;
            }
            remove
            {
                baseTextBox.ContextMenuStripChanged -= value;
            }
        }

        public new event ControlEventHandler ControlAdded
        {
            add
            {
                baseTextBox.ControlAdded += value;
            }
            remove
            {
                baseTextBox.ControlAdded -= value;
            }
        }

        public new event ControlEventHandler ControlRemoved
        {
            add
            {
                baseTextBox.ControlRemoved += value;
            }
            remove
            {
                baseTextBox.ControlRemoved -= value;
            }
        }

        public new event EventHandler CursorChanged
        {
            add
            {
                baseTextBox.CursorChanged += value;
            }
            remove
            {
                baseTextBox.CursorChanged -= value;
            }
        }

        public new event EventHandler Disposed
        {
            add
            {
                baseTextBox.Disposed += value;
            }
            remove
            {
                baseTextBox.Disposed -= value;
            }
        }

        public new event EventHandler DockChanged
        {
            add
            {
                baseTextBox.DockChanged += value;
            }
            remove
            {
                baseTextBox.DockChanged -= value;
            }
        }

        public new event EventHandler DoubleClick
        {
            add
            {
                baseTextBox.DoubleClick += value;
            }
            remove
            {
                baseTextBox.DoubleClick -= value;
            }
        }

        public new event DragEventHandler DragDrop
        {
            add
            {
                baseTextBox.DragDrop += value;
            }
            remove
            {
                baseTextBox.DragDrop -= value;
            }
        }

        public new event DragEventHandler DragEnter
        {
            add
            {
                baseTextBox.DragEnter += value;
            }
            remove
            {
                baseTextBox.DragEnter -= value;
            }
        }

        public new event EventHandler DragLeave
        {
            add
            {
                baseTextBox.DragLeave += value;
            }
            remove
            {
                baseTextBox.DragLeave -= value;
            }
        }

        public new event DragEventHandler DragOver
        {
            add
            {
                baseTextBox.DragOver += value;
            }
            remove
            {
                baseTextBox.DragOver -= value;
            }
        }

        public new event EventHandler EnabledChanged
        {
            add
            {
                baseTextBox.EnabledChanged += value;
            }
            remove
            {
                baseTextBox.EnabledChanged -= value;
            }
        }

        public new event EventHandler Enter
        {
            add
            {
                baseTextBox.Enter += value;
            }
            remove
            {
                baseTextBox.Enter -= value;
            }
        }

        public new event EventHandler FontChanged
        {
            add
            {
                baseTextBox.FontChanged += value;
            }
            remove
            {
                baseTextBox.FontChanged -= value;
            }
        }

        public new event EventHandler ForeColorChanged
        {
            add
            {
                baseTextBox.ForeColorChanged += value;
            }
            remove
            {
                baseTextBox.ForeColorChanged -= value;
            }
        }

        public new event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                baseTextBox.GiveFeedback += value;
            }
            remove
            {
                baseTextBox.GiveFeedback -= value;
            }
        }

        public new event EventHandler GotFocus
        {
            add
            {
                baseTextBox.GotFocus += value;
            }
            remove
            {
                baseTextBox.GotFocus -= value;
            }
        }

        public new event EventHandler HandleCreated
        {
            add
            {
                baseTextBox.HandleCreated += value;
            }
            remove
            {
                baseTextBox.HandleCreated -= value;
            }
        }

        public new event EventHandler HandleDestroyed
        {
            add
            {
                baseTextBox.HandleDestroyed += value;
            }
            remove
            {
                baseTextBox.HandleDestroyed -= value;
            }
        }

        public new event HelpEventHandler HelpRequested
        {
            add
            {
                baseTextBox.HelpRequested += value;
            }
            remove
            {
                baseTextBox.HelpRequested -= value;
            }
        }

        public new event EventHandler ImeModeChanged
        {
            add
            {
                baseTextBox.ImeModeChanged += value;
            }
            remove
            {
                baseTextBox.ImeModeChanged -= value;
            }
        }

        public new event InvalidateEventHandler Invalidated
        {
            add
            {
                baseTextBox.Invalidated += value;
            }
            remove
            {
                baseTextBox.Invalidated -= value;
            }
        }

        public new event KeyEventHandler KeyDown
        {
            add
            {
                baseTextBox.KeyDown += value;
            }
            remove
            {
                baseTextBox.KeyDown -= value;
            }
        }

        public new event KeyPressEventHandler KeyPress
        {
            add
            {
                baseTextBox.KeyPress += value;
            }
            remove
            {
                baseTextBox.KeyPress -= value;
            }
        }

        public new event KeyEventHandler KeyUp
        {
            add
            {
                baseTextBox.KeyUp += value;
            }
            remove
            {
                baseTextBox.KeyUp -= value;
            }
        }

        public new event LayoutEventHandler Layout
        {
            add
            {
                baseTextBox.Layout += value;
            }
            remove
            {
                baseTextBox.Layout -= value;
            }
        }

        public new event EventHandler Leave
        {
            add
            {
                baseTextBox.Leave += value;
            }
            remove
            {
                baseTextBox.Leave -= value;
            }
        }

        public new event EventHandler LocationChanged
        {
            add
            {
                baseTextBox.LocationChanged += value;
            }
            remove
            {
                baseTextBox.LocationChanged -= value;
            }
        }

        public new event EventHandler LostFocus
        {
            add
            {
                baseTextBox.LostFocus += value;
            }
            remove
            {
                baseTextBox.LostFocus -= value;
            }
        }

        public new event EventHandler MarginChanged
        {
            add
            {
                baseTextBox.MarginChanged += value;
            }
            remove
            {
                baseTextBox.MarginChanged -= value;
            }
        }

        public new event EventHandler MouseCaptureChanged
        {
            add
            {
                baseTextBox.MouseCaptureChanged += value;
            }
            remove
            {
                baseTextBox.MouseCaptureChanged -= value;
            }
        }

        public new event MouseEventHandler MouseClick
        {
            add
            {
                baseTextBox.MouseClick += value;
            }
            remove
            {
                baseTextBox.MouseClick -= value;
            }
        }

        public new event MouseEventHandler MouseDoubleClick
        {
            add
            {
                baseTextBox.MouseDoubleClick += value;
            }
            remove
            {
                baseTextBox.MouseDoubleClick -= value;
            }
        }

        public new event MouseEventHandler MouseDown
        {
            add
            {
                baseTextBox.MouseDown += value;
            }
            remove
            {
                baseTextBox.MouseDown -= value;
            }
        }

        public new event EventHandler MouseEnter
        {
            add
            {
                baseTextBox.MouseEnter += value;
            }
            remove
            {
                baseTextBox.MouseEnter -= value;
            }
        }

        public new event EventHandler MouseHover
        {
            add
            {
                baseTextBox.MouseHover += value;
            }
            remove
            {
                baseTextBox.MouseHover -= value;
            }
        }

        public new event EventHandler MouseLeave
        {
            add
            {
                baseTextBox.MouseLeave += value;
            }
            remove
            {
                baseTextBox.MouseLeave -= value;
            }
        }

        public new event MouseEventHandler MouseMove
        {
            add
            {
                baseTextBox.MouseMove += value;
            }
            remove
            {
                baseTextBox.MouseMove -= value;
            }
        }

        public new event MouseEventHandler MouseUp
        {
            add
            {
                baseTextBox.MouseUp += value;
            }
            remove
            {
                baseTextBox.MouseUp -= value;
            }
        }

        public new event MouseEventHandler MouseWheel
        {
            add
            {
                baseTextBox.MouseWheel += value;
            }
            remove
            {
                baseTextBox.MouseWheel -= value;
            }
        }

        public new event EventHandler Move
        {
            add
            {
                baseTextBox.Move += value;
            }
            remove
            {
                baseTextBox.Move -= value;
            }
        }

        public new event EventHandler PaddingChanged
        {
            add
            {
                baseTextBox.PaddingChanged += value;
            }
            remove
            {
                baseTextBox.PaddingChanged -= value;
            }
        }

        public new event PaintEventHandler Paint
        {
            add
            {
                baseTextBox.Paint += value;
            }
            remove
            {
                baseTextBox.Paint -= value;
            }
        }

        public new event EventHandler ParentChanged
        {
            add
            {
                baseTextBox.ParentChanged += value;
            }
            remove
            {
                baseTextBox.ParentChanged -= value;
            }
        }

        public new event PreviewKeyDownEventHandler PreviewKeyDown
        {
            add
            {
                baseTextBox.PreviewKeyDown += value;
            }
            remove
            {
                baseTextBox.PreviewKeyDown -= value;
            }
        }

        public new event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp
        {
            add
            {
                baseTextBox.QueryAccessibilityHelp += value;
            }
            remove
            {
                baseTextBox.QueryAccessibilityHelp -= value;
            }
        }

        public new event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                baseTextBox.QueryContinueDrag += value;
            }
            remove
            {
                baseTextBox.QueryContinueDrag -= value;
            }
        }

        public new event EventHandler RegionChanged
        {
            add
            {
                baseTextBox.RegionChanged += value;
            }
            remove
            {
                baseTextBox.RegionChanged -= value;
            }
        }

        public new event EventHandler Resize
        {
            add
            {
                baseTextBox.Resize += value;
            }
            remove
            {
                baseTextBox.Resize -= value;
            }
        }

        public new event EventHandler RightToLeftChanged
        {
            add
            {
                baseTextBox.RightToLeftChanged += value;
            }
            remove
            {
                baseTextBox.RightToLeftChanged -= value;
            }
        }

        public new event EventHandler SizeChanged
        {
            add
            {
                baseTextBox.SizeChanged += value;
            }
            remove
            {
                baseTextBox.SizeChanged -= value;
            }
        }

        public new event EventHandler StyleChanged
        {
            add
            {
                baseTextBox.StyleChanged += value;
            }
            remove
            {
                baseTextBox.StyleChanged -= value;
            }
        }

        public new event EventHandler SystemColorsChanged
        {
            add
            {
                baseTextBox.SystemColorsChanged += value;
            }
            remove
            {
                baseTextBox.SystemColorsChanged -= value;
            }
        }

        public new event EventHandler TabIndexChanged
        {
            add
            {
                baseTextBox.TabIndexChanged += value;
            }
            remove
            {
                baseTextBox.TabIndexChanged -= value;
            }
        }

        public new event EventHandler TabStopChanged
        {
            add
            {
                baseTextBox.TabStopChanged += value;
            }
            remove
            {
                baseTextBox.TabStopChanged -= value;
            }
        }

        public new event EventHandler TextChanged
        {
            add
            {
                baseTextBox.TextChanged += value;
            }
            remove
            {
                baseTextBox.TextChanged -= value;
            }
        }

        public new event EventHandler Validated
        {
            add
            {
                baseTextBox.Validated += value;
            }
            remove
            {
                baseTextBox.Validated -= value;
            }
        }

        public new event CancelEventHandler Validating
        {
            add
            {
                baseTextBox.Validating += value;
            }
            remove
            {
                baseTextBox.Validating -= value;
            }
        }

        public new event EventHandler VisibleChanged
        {
            add
            {
                baseTextBox.VisibleChanged += value;
            }
            remove
            {
                baseTextBox.VisibleChanged -= value;
            }
        }
        # endregion

        private readonly AnimationManager _animationManager;

        public bool isFocused = false;
        private const int PREFIX_SUFFIX_PADDING = 4;
        private const int ICON_SIZE = 24;
        private const int HINT_TEXT_SMALL_SIZE = 18;
        private const int HINT_TEXT_SMALL_Y = 4;
        private const int LEFT_PADDING = 16;
        private const int RIGHT_PADDING = 12;
        private const int ACTIVATION_INDICATOR_HEIGHT = 2;
        private const int HELPER_TEXT_HEIGHT = 16;
        private const int FONT_HEIGHT = 20;
        
        private int HEIGHT = 48;

        private int LINE_Y;
        private bool hasHint;
        private bool _errorState = false;
        private int _left_padding;
        private int _right_padding;
        private int _prefix_padding;
        private int _suffix_padding;
        private int _helperTextHeight;
        private bool _upIconDown;
        private bool _downIconDown;
        private Rectangle _upIconBounds;
        private Rectangle _downIconBounds;

        private Dictionary<string, TextureBrush> iconsBrushes;
        private Dictionary<string, TextureBrush> iconsErrorBrushes;

        protected readonly BaseTextBox baseTextBox;

        public MaterialNumericUpDown()
        {
            // Material Properties
            UseAccent = true;
            MouseState = MouseState.OUT;

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer, true);

            // Animations
            _animationManager = new AnimationManager
            {
                Increment = 0.06,
                AnimationType = AnimationType.EaseInOut,
                InterruptAnimation = false
            };
            _animationManager.OnAnimationProgress += sender => Invalidate();

            SkinManager.ColorSchemeChanged += sender =>
            {
                preProcessIcons();
            };

            SkinManager.ThemeChanged += sender =>
            {
                preProcessIcons();
            };

            Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle1);

            baseTextBox = new BaseTextBox
            {
                BorderStyle = BorderStyle.None,
                Font = base.Font,
                ForeColor = SkinManager.TextHighEmphasisColor,
                Multiline = false,
                Location = new Point(LEFT_PADDING, HEIGHT/2- FONT_HEIGHT/2),
                Width = Width - (LEFT_PADDING + RIGHT_PADDING),
                Height = FONT_HEIGHT
            };

            Enabled = true;
            ReadOnly = false;
            Size = new Size(250, HEIGHT);

            UseTallSize = true;
            PrefixSuffix = PrefixSuffixTypes.None;
            ShowAssistiveText = false;
            HelperText = string.Empty;
            ErrorMessage = string.Empty;

            if (!Controls.Contains(baseTextBox) && !DesignMode)
            {
                Controls.Add(baseTextBox);
            }

            baseTextBox.GotFocus += (sender, args) =>
            {
                if (Enabled)
                {
                    isFocused = true;
                    _animationManager.StartNewAnimation(AnimationDirection.In);
                }
                else
                    base.Focus();
                UpdateRects();
            };
            baseTextBox.LostFocus += (sender, args) =>
            {
                isFocused = false;
                _animationManager.StartNewAnimation(AnimationDirection.Out);
                UpdateRects();
            };

            baseTextBox.Leave += new EventHandler(ParseTextToValue);
            baseTextBox.TextChanged += new EventHandler(Redraw);
            baseTextBox.BackColorChanged += new EventHandler(Redraw);

            baseTextBox.TabStop = true;
            this.TabStop = false;

            cms.Opening += ContextMenuStripOnOpening;
            cms.OnItemClickStart += ContextMenuStripOnItemClickStart;
            ContextMenuStrip = cms;
        }

        private void ParseTextToValue(object sencer, EventArgs e)
        {
            if (decimal.TryParse(baseTextBox.Text, out var value))
            {
                if (value > Maximum)
                    value = Maximum;
                if (value < Minimum)
                    value = Minimum;
                Value = value;
            }
        }

        private void Redraw(object sencer, EventArgs e)
        {
            SuspendLayout();
            Invalidate();
            ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Parent.BackColor);
            SolidBrush backBrush = new SolidBrush(DrawHelper.BlendColor(Parent.BackColor, SkinManager.BackgroundAlternativeColor, SkinManager.BackgroundAlternativeColor.A));
            
            //backColor
            g.FillRectangle(
                !Enabled ? SkinManager.BackgroundDisabledBrush : // Disabled
                isFocused ? SkinManager.BackgroundFocusBrush :  // Focused
                MouseState == MouseState.HOVER && (!ReadOnly || (ReadOnly && !AnimateReadOnly)) ? SkinManager.BackgroundHoverBrush : // Hover
                backBrush, // Normal
                ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, LINE_Y);

            baseTextBox.BackColor = !Enabled ? ColorHelper.RemoveAlpha(SkinManager.BackgroundDisabledColor, BackColor) : //Disabled
                isFocused ? DrawHelper.BlendColor(BackColor, SkinManager.BackgroundFocusColor, SkinManager.BackgroundFocusColor.A) : //Focused
                MouseState == MouseState.HOVER && (!ReadOnly || (ReadOnly && !AnimateReadOnly)) ? DrawHelper.BlendColor(BackColor, SkinManager.BackgroundHoverColor, SkinManager.BackgroundHoverColor.A) : // Hover
                DrawHelper.BlendColor(BackColor, SkinManager.BackgroundAlternativeColor, SkinManager.BackgroundAlternativeColor.A); // Normal

            // HintText
            bool userTextPresent = !String.IsNullOrEmpty(Text);
            Rectangle helperTextRect = new Rectangle(LEFT_PADDING - _prefix_padding, LINE_Y + ACTIVATION_INDICATOR_HEIGHT, Width - (LEFT_PADDING - _prefix_padding) - _right_padding, HELPER_TEXT_HEIGHT);
            Rectangle hintRect = new Rectangle(_left_padding - _prefix_padding, HINT_TEXT_SMALL_Y, Width - (_left_padding - _prefix_padding) - _right_padding, HINT_TEXT_SMALL_SIZE);
            int hintTextSize = 12;

            // bottom line base
            g.FillRectangle(SkinManager.DividersAlternativeBrush, 0, LINE_Y, Width, 1);

            if (ReadOnly == false || (ReadOnly && AnimateReadOnly))
            {
                if (!_animationManager.IsAnimating())
                {
                    // No animation

                    // bottom line
                    if (isFocused)
                    {
                        g.FillRectangle(_errorState ? SkinManager.BackgroundHoverRedBrush : isFocused ? UseAccent ? SkinManager.ColorScheme.AccentBrush : SkinManager.ColorScheme.PrimaryBrush : SkinManager.DividersBrush, 0, LINE_Y, Width, isFocused ? 2 : 1);
                    }
                }
                else
                {
                    // Animate - Focus got/lost
                    double animationProgress = _animationManager.GetProgress();

                    // Line Animation
                    int LineAnimationWidth = (int)(Width * animationProgress);
                    int LineAnimationX = (Width / 2) - (LineAnimationWidth / 2);
                    g.FillRectangle(UseAccent ? SkinManager.ColorScheme.AccentBrush : SkinManager.ColorScheme.PrimaryBrush, LineAnimationX, LINE_Y, LineAnimationWidth, 2);
                }
            }

            // Prefix:
            if (_prefixsuffix == PrefixSuffixTypes.Prefix && _prefixsuffixText != null && _prefixsuffixText.Length > 0 && (isFocused || userTextPresent || !hasHint))
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    Rectangle prefixRect = new Rectangle(
                        _left_padding - _prefix_padding,
                        hasHint && UseTallSize ? (hintRect.Y + hintRect.Height) - 2 : ClientRectangle.Y,
//                        NativeText.MeasureLogString(_prefixsuffixText, SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1)).Width,
                        _prefix_padding,
                        hasHint && UseTallSize ? LINE_Y - (hintRect.Y + hintRect.Height) : LINE_Y);

                    // Draw Prefix text 
                    NativeText.DrawTransparentText(
                    _prefixsuffixText,
                    SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1),
                    Enabled ? SkinManager.TextMediumEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    prefixRect.Location,
                    prefixRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }

            // Suffix:
            if (_prefixsuffix == PrefixSuffixTypes.Suffix && _prefixsuffixText != null && _prefixsuffixText.Length > 0 && (isFocused || userTextPresent || !hasHint))
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    Rectangle suffixRect = new Rectangle(
                        Width - _right_padding ,
                        hasHint && UseTallSize ? (hintRect.Y + hintRect.Height) - 2 : ClientRectangle.Y,
                        //NativeText.MeasureLogString(_prefixsuffixText, SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1)).Width + PREFIX_SUFFIX_PADDING,
                        _suffix_padding,
                        hasHint && UseTallSize ? LINE_Y - (hintRect.Y + hintRect.Height) : LINE_Y);

                    // Draw Suffix text 
                    NativeText.DrawTransparentText(
                    _prefixsuffixText,
                    SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1),
                    Enabled ? SkinManager.TextMediumEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    suffixRect.Location,
                    suffixRect.Size,
                    NativeTextRenderer.TextAlignFlags.Right | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }

            // Draw hint text
            if(hasHint && UseTallSize && (isFocused || userTextPresent))
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    NativeText.DrawTransparentText(
                    "",
                    SkinManager.getTextBoxFontBySize(hintTextSize),
                    Enabled ? !_errorState || (!userTextPresent && !isFocused) ? isFocused ? UseAccent ?
                    SkinManager.ColorScheme.AccentColor : // Focus Accent
                    SkinManager.ColorScheme.PrimaryColor : // Focus Primary
                    SkinManager.TextMediumEmphasisColor : // not focused
                    SkinManager.BackgroundHoverRedColor : // error state
                    SkinManager.TextDisabledOrHintColor, // Disabled
                    hintRect.Location,
                    hintRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }

            // Draw helper text
            if (_showAssistiveText && isFocused && !_errorState)
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    NativeText.DrawTransparentText(
                    HelperText,
                    SkinManager.getTextBoxFontBySize(hintTextSize),
                    Enabled ? !_errorState || (!userTextPresent && !isFocused) ? isFocused ? UseAccent ?
                    SkinManager.ColorScheme.AccentColor : // Focus Accent
                    SkinManager.ColorScheme.PrimaryColor : // Focus Primary
                    SkinManager.TextMediumEmphasisColor : // not focused
                    SkinManager.BackgroundHoverRedColor : // error state
                    SkinManager.TextDisabledOrHintColor, // Disabled
                    helperTextRect.Location,
                    helperTextRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }

            // Draw error message
            if (_showAssistiveText && _errorState && ErrorMessage!=null)
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    NativeText.DrawTransparentText(
                    ErrorMessage,
                    SkinManager.getTextBoxFontBySize(hintTextSize),
                    Enabled ? 
                    SkinManager.BackgroundHoverRedColor : // error state
                    SkinManager.TextDisabledOrHintColor, // Disabled
                    helperTextRect.Location,
                    helperTextRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }

            //UpDown Icon
            if (UpIcon != null || DownIcon != null)
            {
                if (_errorState)
                {
                    g.FillRectangle(iconsErrorBrushes["_downIcon"], _downIconBounds);
                    g.FillRectangle(iconsErrorBrushes["_upIcon"], _upIconBounds);
                }
                else
                {
                    g.FillRectangle(iconsBrushes["_downIcon"], _downIconBounds);
                    g.FillRectangle(iconsBrushes["_upIcon"], _upIconBounds);
                }
            }

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (DesignMode)
                return;

            Cursor = Cursors.IBeam;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (DesignMode)
                return;

            _upIconDown = false;
            _downIconDown = false;

            if (this._upIconBounds.Contains(e.Location))
            {
                var newValue = Value + Increment;
                if (newValue > Maximum)
                    newValue = Maximum;
                Value = newValue;
                baseTextBox.Text = Value.ToString(CultureInfo.InvariantCulture);
            }
            else if (this._downIconBounds.Contains(e.Location))
            {
                var newValue = Value - Increment;
                if (newValue < Minimum)
                    newValue = Minimum;
                Value = newValue;
                baseTextBox.Text = Value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                base.OnMouseUp(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (DesignMode)
                return;

            if (this._upIconBounds.Contains(e.Location))
            {
                _upIconDown = true;
                _downIconDown = false;
            }
            else if (this._downIconBounds.Contains(e.Location))
            {
                _upIconDown = false;
                _downIconDown = true;
            }
            else
            {
                _upIconDown = false;
                _downIconDown = false;

                baseTextBox?.Focus();
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (DesignMode)
                return;

            base.OnMouseEnter(e);
            MouseState = MouseState.HOVER;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (DesignMode)
                return;

            if (this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                return;
            else
            {
                base.OnMouseLeave(e);
                MouseState = MouseState.OUT;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateRects();
            preProcessIcons();

            Size = new Size(Width, HEIGHT);
            LINE_Y = HEIGHT - ACTIVATION_INDICATOR_HEIGHT - _helperTextHeight;

        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            // events
            MouseState = MouseState.OUT;

        }

        #region Icon

        private static Size ResizeIcon(Image Icon)
        {
            int newWidth, newHeight;
            //Resize icon if greater than ICON_SIZE
            if (Icon.Width > ICON_SIZE || Icon.Height > ICON_SIZE)
            {
                //calculate aspect ratio
                float aspect = Icon.Width / (float)Icon.Height;

                //calculate new dimensions based on aspect ratio
                newWidth = (int)(ICON_SIZE * aspect);
                newHeight = (int)(newWidth / aspect);

                //if one of the two dimensions exceed the box dimensions
                if (newWidth > ICON_SIZE || newHeight > ICON_SIZE)
                {
                    //depending on which of the two exceeds the box dimensions set it as the box dimension and calculate the other one based on the aspect ratio
                    if (newWidth > newHeight)
                    {
                        newWidth = ICON_SIZE;
                        newHeight = (int)(newWidth / aspect);
                    }
                    else
                    {
                        newHeight = ICON_SIZE;
                        newWidth = (int)(newHeight * aspect);
                    }
                }
            }
            else
            {
                newWidth = Icon.Width;
                newHeight = Icon.Height;
            }

            return new Size()
            {
                Height = newHeight,
                Width = newWidth
            };
        }

        private void preProcessIcons()
        {
            if (_upIcon == null && _downIcon == null) return;

            // Calculate lightness and color
            float l = (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT) ? 0f : 1f;

            // Create matrices
            float[][] matrixGray = {
                    new float[] {   0,   0,   0,   0,  0}, // Red scale factor
                    new float[] {   0,   0,   0,   0,  0}, // Green scale factor
                    new float[] {   0,   0,   0,   0,  0}, // Blue scale factor
                    new float[] {   0,   0,   0, Enabled ? .7f : .3f,  0}, // alpha scale factor
                    new float[] {   l,   l,   l,   0,  1}};// offset

            float[][] matrixRed = {
                    new float[] {   0,   0,   0,   0,  0}, // Red scale factor
                    new float[] {   0,   0,   0,   0,  0}, // Green scale factor
                    new float[] {   0,   0,   0,   0,  0}, // Blue scale factor
                    new float[] {   0,   0,   0,   1,  0}, // alpha scale factor
                    new float[] {   1,   0,   0,   0,  1}};// offset

            ColorMatrix colorMatrixGray = new ColorMatrix(matrixGray);
            ColorMatrix colorMatrixRed = new ColorMatrix(matrixRed);

            ImageAttributes grayImageAttributes = new ImageAttributes();
            ImageAttributes redImageAttributes = new ImageAttributes();

            // Set color matrices
            grayImageAttributes.SetColorMatrix(colorMatrixGray, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            redImageAttributes.SetColorMatrix(colorMatrixRed, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Create brushes
            iconsBrushes = new Dictionary<string, TextureBrush>(2);
            iconsErrorBrushes = new Dictionary<string, TextureBrush>(2);

            // Image Rect
            Rectangle destRect = new Rectangle(0, 0, ICON_SIZE, ICON_SIZE);

            if (_downIcon != null)
            {
                // ********************
                // *** _downIcon ***
                // ********************

                //Resize icon if greater than ICON_SIZE
                Size newSize_downIcon = ResizeIcon(_downIcon);
                Bitmap _downIconIconResized = new Bitmap(_downIcon, newSize_downIcon.Width, newSize_downIcon.Height);

                // Create a pre-processed copy of the image (GRAY)
                Bitmap bgray = new Bitmap(destRect.Width, destRect.Height);
                using (Graphics gGray = Graphics.FromImage(bgray))
                {
                    gGray.DrawImage(_downIconIconResized,
                        new Point[] {
                                    new Point(0, 0),
                                    new Point(destRect.Width, 0),
                                    new Point(0, destRect.Height),
                        },
                        destRect, GraphicsUnit.Pixel, grayImageAttributes);
                }

                //Create a pre - processed copy of the image(RED)
                Bitmap bred = new Bitmap(destRect.Width, destRect.Height);
                using (Graphics gred = Graphics.FromImage(bred))
                {
                    gred.DrawImage(_downIconIconResized,
                        new Point[] {
                                    new Point(0, 0),
                                    new Point(destRect.Width, 0),
                                    new Point(0, destRect.Height),
                        },
                        destRect, GraphicsUnit.Pixel, redImageAttributes);
                }

                // added processed image to brush for drawing
                TextureBrush textureBrushGray = new TextureBrush(bgray);
                TextureBrush textureBrushRed = new TextureBrush(bred);

                textureBrushGray.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                textureBrushRed.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;

                var iconRect = _downIconBounds;

                textureBrushGray.TranslateTransform(iconRect.X + iconRect.Width / 2 - _downIconIconResized.Width / 2,
                                                    iconRect.Y + iconRect.Height / 2 - _downIconIconResized.Height / 2);
                textureBrushRed.TranslateTransform(iconRect.X + iconRect.Width / 2 - _downIconIconResized.Width / 2,
                                                     iconRect.Y + iconRect.Height / 2 - _downIconIconResized.Height / 2);

                // add to dictionary
                iconsBrushes.Add("_downIcon", textureBrushGray);

                iconsErrorBrushes.Add("_downIcon", textureBrushRed);

            }

            if (_upIcon != null)
            {
                // *********************
                // *** _upIcon ***
                // *********************

                //Resize icon if greater than ICON_SIZE
                Size newSize_upIcon = ResizeIcon(_upIcon);
                Bitmap _upIconResized = new Bitmap(_upIcon, newSize_upIcon.Width, newSize_upIcon.Height);

                // Create a pre-processed copy of the image (GRAY)
                Bitmap bgray = new Bitmap(destRect.Width, destRect.Height);
                using (Graphics gGray = Graphics.FromImage(bgray))
                {
                    gGray.DrawImage(_upIconResized,
                        new Point[] {
                                    new Point(0, 0),
                                    new Point(destRect.Width, 0),
                                    new Point(0, destRect.Height),
                        },
                        destRect, GraphicsUnit.Pixel, grayImageAttributes);
                }

                //Create a pre - processed copy of the image(RED)
                Bitmap bred = new Bitmap(destRect.Width, destRect.Height);
                using (Graphics gred = Graphics.FromImage(bred))
                {
                    gred.DrawImage(_upIconResized,
                        new Point[] {
                                    new Point(0, 0),
                                    new Point(destRect.Width, 0),
                                    new Point(0, destRect.Height),
                        },
                        destRect, GraphicsUnit.Pixel, redImageAttributes);
                }


                // added processed image to brush for drawing
                TextureBrush textureBrushGray = new TextureBrush(bgray);
                TextureBrush textureBrushRed = new TextureBrush(bred);

                textureBrushGray.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                textureBrushRed.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;

                var iconRect = _upIconBounds;

                textureBrushGray.TranslateTransform(iconRect.X + iconRect.Width / 2 - _upIconResized.Width / 2,
                                                    iconRect.Y + iconRect.Height / 2 - _upIconResized.Height / 2);
                textureBrushRed.TranslateTransform(iconRect.X + iconRect.Width / 2 - _upIconResized.Width / 2,
                                                     iconRect.Y + iconRect.Height / 2 - _upIconResized.Height / 2);

                // add to dictionary
                iconsBrushes.Add("_upIcon", textureBrushGray);
                //iconsSelectedBrushes.Add(0, textureBrushColor);
                iconsErrorBrushes.Add("_upIcon", textureBrushRed);
            }
        }

        #endregion

        private void UpdateHeight()
        {
            HEIGHT = _UseTallSize ? 48 : 36;
            HEIGHT += _helperTextHeight;
            Size = new Size(Size.Width, HEIGHT);
        }

        private void UpdateRects()
        {
            _left_padding = LEFT_PADDING;
            _right_padding = ICON_SIZE;

            if (_prefixsuffix == PrefixSuffixTypes.Prefix && _prefixsuffixText != null && _prefixsuffixText.Length > 0)
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(CreateGraphics()))
                {
                    _prefix_padding = NativeText.MeasureLogString(_prefixsuffixText, SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1)).Width + PREFIX_SUFFIX_PADDING;
                    _left_padding += _prefix_padding;
                }
            }
            else
                _prefix_padding = 0;
                
            if (_prefixsuffix == PrefixSuffixTypes.Suffix && _prefixsuffixText != null && _prefixsuffixText.Length > 0)
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(CreateGraphics()))
                {
                    _suffix_padding = NativeText.MeasureLogString(_prefixsuffixText, SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1)).Width + PREFIX_SUFFIX_PADDING;
                    _right_padding += _suffix_padding;
                }
            }
            else
                _suffix_padding = 0;

            if (hasHint && UseTallSize && (isFocused || !String.IsNullOrEmpty(Text)))
            {
                baseTextBox.Location = new Point(_left_padding, 22);
                baseTextBox.Width = Width - (_left_padding + _right_padding);
                baseTextBox.Height = FONT_HEIGHT;
            }
            else
            {
                baseTextBox.Location = new Point(_left_padding, (LINE_Y + ACTIVATION_INDICATOR_HEIGHT) / 2 - FONT_HEIGHT / 2);
                baseTextBox.Width = Width - (_left_padding + _right_padding);
                baseTextBox.Height = FONT_HEIGHT;
            }

            var iconArea = (HEIGHT - _helperTextHeight) / 2;
            var space = (iconArea * 2 - ICON_SIZE * 2) / 3;
            _upIconBounds = new Rectangle(baseTextBox.Right, space, ICON_SIZE, ICON_SIZE);
            _downIconBounds = new Rectangle(baseTextBox.Right, space + ICON_SIZE + space, ICON_SIZE, ICON_SIZE);
        }

        public void SetErrorState(bool ErrorState)
        {
            _errorState = ErrorState;
            if (_errorState)
                baseTextBox.ForeColor = SkinManager.BackgroundHoverRedColor;
            else
                baseTextBox.ForeColor = SkinManager.TextHighEmphasisColor;
            baseTextBox.Invalidate();
            Invalidate();
        }

        public bool GetErrorState()
        {
            return _errorState;
        }

        private void ContextMenuStripOnItemClickStart(object sender, ToolStripItemClickedEventArgs toolStripItemClickedEventArgs)
        {
            switch (toolStripItemClickedEventArgs.ClickedItem.Text)
            {
                case "Undo":
                    Undo();
                    break;
                case "Cut":
                    Cut();
                    break;
                case "Copy":
                    Copy();
                    break;
                case "Paste":
                    Paste();
                    break;
                case "Delete":
                    SelectedText = string.Empty;
                    break;
                case "Select All":
                    SelectAll();
                    break;
            }
        }

        private void ContextMenuStripOnOpening(object sender, CancelEventArgs cancelEventArgs)
        {
            if (sender is BaseTextBoxContextMenuStrip strip)
            {
                strip.undo.Enabled = baseTextBox.CanUndo && !ReadOnly;
                strip.cut.Enabled = !string.IsNullOrEmpty(SelectedText) && !ReadOnly;
                strip.copy.Enabled = !string.IsNullOrEmpty(SelectedText);
                strip.paste.Enabled = Clipboard.ContainsText() && !ReadOnly;
                strip.delete.Enabled = !string.IsNullOrEmpty(SelectedText) && !ReadOnly;
                strip.selectAll.Enabled = !string.IsNullOrEmpty(Text);
            }
        }

        private void LeaveOnEnterKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SendKeys.Send("{TAB}");
            }
        }
    }
}
