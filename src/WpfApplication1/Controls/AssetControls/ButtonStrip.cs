using AssetBuilder.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder.AssetControls
{
    public class ButtonStrip : UserControl
    {
        public Button btnReleaseConclusionMap;
        public Button btnNaturalLanguage;
        public Button btnAdd;
		public Button btnEdit;
        public Button btnDuplicate;
        public Button btnDelete;
        public Button btnRefresh;
        public Button btnClose;
        public Button btnUsage;
        //public Button btnNLW;
        public Button btnDeriveAnswer;
        public Button btnDeriveQuestion;
        public Button btnCreateAnswer;
        public Button btnProperties;
        public Button btnAudit;
        public Button btnCancel;
        public Button btnSave;
        public Button btnFind;
        countIndicator ci;
        internal assetControl asset;

        public string PropertyCount
        {
            get
            {
                return ci.Number;
            }
            set
            {
                ci.Number = value;
            }
        }

        public bool DefaultProperties
        {
            get
            {
                return ci.ShowZero;
            }
            set
            {
                ci.ShowZero = value;
            }
        }

        static RoutedEvent RegisterRoutedEvent(string name)
        {
            return EventManager.RegisterRoutedEvent(name, RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ButtonStrip));
        }

        static ButtonStrip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonStrip), new FrameworkPropertyMetadata(typeof(ButtonStrip)));

            ReleaseConclusionMapEvent = RegisterRoutedEvent("btnReleaseConclusionMapToolTip");
            NaturalLanguageEvent = RegisterRoutedEvent("btnNaturalLanguage");
            AddEvent = RegisterRoutedEvent("btnAdd");
			EditEvent = RegisterRoutedEvent("btnEdit");
            DuplicateEvent = RegisterRoutedEvent("btnDuplicate");
            DeleteEvent = RegisterRoutedEvent("btnDelete");
            RefreshEvent = RegisterRoutedEvent("btnRefresh");
            CloseEvent = RegisterRoutedEvent("btnClose");
            UsageEvent = RegisterRoutedEvent("btnUsage");
            //NLWEvent = RegisterRoutedEvent("btnNLW");
            DeriveAnswerEvent = RegisterRoutedEvent("btnDeriveAnswer");
            DeriveQuestionEvent = RegisterRoutedEvent("btnDeriveQuestion");
            CreateAnswerEvent = RegisterRoutedEvent("btnCreateAnswer");
            PropertiesEvent = RegisterRoutedEvent("btnProperties");
            CancelEvent = RegisterRoutedEvent("btnCancel");
            SaveEvent = RegisterRoutedEvent("btnSave");
            FindEvent = RegisterRoutedEvent("btnFind");
            AuditEvent = RegisterRoutedEvent("btnAudit");
        }

        public ButtonStrip()
        {
        }
        
        private RoutedEventHandler GetHandler(Action action)
        {
            return new RoutedEventHandler(delegate(object sender, RoutedEventArgs e) { action(); });
        }

        private Button GetTemplateButton(string name)
        {
            return this.GetTemplateChild(name) as Button;
        }

        private void MapButtons()
        {
            btnReleaseConclusionMap = GetTemplateButton("btnReleaseConclusionMap");
            btnNaturalLanguage = GetTemplateButton("btnNaturalLanguage");
            btnAdd = GetTemplateButton("btnAdd");
			btnEdit = GetTemplateButton("btnEdit");
            btnDuplicate = GetTemplateButton("btnDuplicate");
            btnDelete = GetTemplateButton("btnDelete");
            btnRefresh = GetTemplateButton("btnRefresh");
            btnClose = GetTemplateButton("btnClose");
            btnUsage = GetTemplateButton("btnUsage");
            //btnNLW = GetTemplateButton("btnNLW");
            btnDeriveAnswer = GetTemplateButton("btnDeriveAnswer");
            btnDeriveQuestion = GetTemplateButton("btnDeriveQuestion");
            btnCreateAnswer = GetTemplateButton("btnCreateAnswer");
            btnProperties = GetTemplateButton("btnProperties");
            btnAudit = GetTemplateButton("btnAudit");
            btnCancel = GetTemplateButton("btnCancel");
            btnSave = GetTemplateButton("btnSave");
            btnFind = GetTemplateButton("btnFind");
            ci = (countIndicator)GetTemplateChild("PropertyCount");
        }

        private void SetUpButtonEvents()
        {
            btnReleaseConclusionMap.Click += GetHandler(OnReleaseConclusionMap);
            btnNaturalLanguage.Click += GetHandler(OnNaturalLanguage);
            btnAdd.Click += GetHandler(OnAdd);
            btnEdit.Click += GetHandler(OnEdit);
            btnDuplicate.Click += GetHandler(OnDuplicate);
            btnDelete.Click += GetHandler(OnDelete);
            btnRefresh.Click += GetHandler(OnRefresh);
            btnClose.Click += GetHandler(OnClose);
            btnUsage.Click += GetHandler(OnUsage);
            //btnNLW.Click += GetHandler(OnNLW);
            btnDeriveAnswer.Click += GetHandler(OnDeriveAnswer);
            btnDeriveQuestion.Click += GetHandler(OnDeriveQuestion);
            btnCreateAnswer.Click += GetHandler(OnCreateAnswer);
            btnProperties.Click += GetHandler(OnProperties);
            btnCancel.Click += GetHandler(OnCancel);
            btnSave.Click += GetHandler(OnSave);
            btnFind.Click += GetHandler(OnFind);
            btnAudit.Click += GetHandler(OnAudit);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            MapButtons();

            SetUpButtonEvents();
        }

        private void setButtonsVisibility(Visibility visibility, params Button[] buttons)
        {
            foreach (var button in buttons)
            {
                button.Visibility = visibility;
            }
        }

        public void setButtons(bool IsEditMode)
        {
            Visibility viewing = Visibility.Visible;
            Visibility editing = Visibility.Collapsed;

            if (IsEditMode)
            {
                viewing = Visibility.Collapsed;
                editing = Visibility.Visible;
            }

            setButtonsVisibility(viewing, 
                btnAdd, btnEdit, btnDuplicate, btnDelete, btnRefresh,
                btnClose, btnUsage, btnDeriveAnswer, btnDeriveQuestion, btnCreateAnswer, btnProperties);

            setButtonsVisibility(editing,
                btnCancel, btnSave, btnFind); //, btnNLW);

            if (asset != null)
            {
				btnNaturalLanguage.IsEnabled = true;
                switch (asset.assetType)
                {
                    case AssetType.Title:
                        btnEdit.IsEnabled = false;
                        setButtonsVisibility(Visibility.Collapsed,
                            btnAdd, btnDuplicate, btnDelete, //btnNLW,
                            btnDeriveAnswer, btnDeriveQuestion, btnCreateAnswer, btnProperties);
                        break;
                    case AssetType.Group:
                        btnEdit.IsEnabled = false;
                        setButtonsVisibility(Visibility.Collapsed,
                            btnAdd, btnDuplicate, btnDelete, //btnNLW,
                            btnDeriveAnswer, btnDeriveQuestion, btnCreateAnswer, btnProperties);
                        break;
                    case AssetType.Question:
                        //btnDeriveQuestion.IsEnabled = true;
                        btnDeriveAnswer.IsEnabled = true;
                        btnCreateAnswer.IsEnabled = true;
                        break;
                    case AssetType.Conclusion:
                        btnDeriveQuestion.IsEnabled = false;
                        btnDeriveAnswer.IsEnabled = true;
                        btnCreateAnswer.IsEnabled = false;
                        break;
                    case AssetType.ConclusionMap:
                        setButtonsVisibility(Visibility.Collapsed, btnNaturalLanguage);
                        //setButtonsVisibility(viewing, btnReleaseConclusionMap);
                        btnDeriveQuestion.IsEnabled = false;
                        btnDeriveAnswer.IsEnabled = false;
                        btnCreateAnswer.IsEnabled = false;
                        break;
                    case AssetType.Algo:
                    case AssetType.Answer:
                    case AssetType.Bullet:
                    default:
                        btnDeriveQuestion.IsEnabled = false;
                        btnDeriveAnswer.IsEnabled = false;
                        btnCreateAnswer.IsEnabled = false;
                        break;
                }
            }

            if (!Window1.AllowProperties) btnProperties.Visibility = Visibility.Collapsed;
            if (!Window1.AllowAudit) btnAudit.Visibility = Visibility.Collapsed;

            if (Window1.IsReviewer || Window1.IsTranslator)
			{
				btnAdd.IsEnabled = false;
                btnEdit.IsEnabled = Window1.IsEditor;
				btnDelete.IsEnabled = false;
				btnDuplicate.IsEnabled = false;
				btnDeriveAnswer.IsEnabled = false;
				btnDeriveQuestion.IsEnabled = false;
				btnCreateAnswer.IsEnabled = false;
                btnProperties.IsEnabled = false;
			}
			else if (Window1.IsEditor)
			{
				btnAdd.IsEnabled = false;
				btnDelete.IsEnabled = false;
				btnDuplicate.IsEnabled = false;
				btnDeriveAnswer.IsEnabled = false;
				btnDeriveQuestion.IsEnabled = false;
				btnCreateAnswer.IsEnabled = false;
			}
			else if (Window1.EditTranslation) btnEdit.IsEnabled = true;
        }

        #region Routed Events

        public static RoutedEvent ReleaseConclusionMapEvent;

        public event RoutedEventHandler ReleaseConclusionMap
        {
            add { AddHandler(ReleaseConclusionMapEvent, value); }
            remove { RemoveHandler(ReleaseConclusionMapEvent, value); }
        }

        protected virtual void OnReleaseConclusionMap()
		{
			RoutedEventArgs args = new RoutedEventArgs();
			args.RoutedEvent = ReleaseConclusionMapEvent;
			RaiseEvent(args);
		}

        public static RoutedEvent NaturalLanguageEvent;

        public event RoutedEventHandler NaturalLanguage
        {
            add { AddHandler(NaturalLanguageEvent, value); }
            remove { RemoveHandler(NaturalLanguageEvent, value); }
        }

        protected virtual void OnNaturalLanguage()
		{
			RoutedEventArgs args = new RoutedEventArgs();
			args.RoutedEvent = NaturalLanguageEvent;
			RaiseEvent(args);
		}

		public static RoutedEvent AddEvent;

		public event RoutedEventHandler Add
		{
			add { AddHandler(AddEvent, value); }
			remove { RemoveHandler(AddEvent, value); }
		}

		protected virtual void OnAdd()
		{
			RoutedEventArgs args = new RoutedEventArgs();
			args.RoutedEvent = AddEvent;
			RaiseEvent(args);
		}

		public static RoutedEvent EditEvent;

        public event RoutedEventHandler Edit
        {
            add { AddHandler(EditEvent, value); }
            remove { RemoveHandler(EditEvent, value); }
        }

        protected virtual void OnEdit()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = EditEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent DuplicateEvent;

        public event RoutedEventHandler Duplicate
        {
            add { AddHandler(DuplicateEvent, value); }
            remove { RemoveHandler(DuplicateEvent, value); }
        }

        protected virtual void OnDuplicate()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = DuplicateEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent DeleteEvent;

        public event RoutedEventHandler Delete
        {
            add { AddHandler(DeleteEvent, value); }
            remove { RemoveHandler(DeleteEvent, value); }
        }

        protected virtual void OnDelete()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = DeleteEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent RefreshEvent;

        public event RoutedEventHandler Refresh
        {
            add { AddHandler(RefreshEvent, value); }
            remove { RemoveHandler(RefreshEvent, value); }
        }

        protected virtual void OnRefresh()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = RefreshEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent CloseEvent;

        public event RoutedEventHandler Close
        {
            add { AddHandler(CloseEvent, value); }
            remove { RemoveHandler(CloseEvent, value); }
        }

        protected virtual void OnClose()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CloseEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent UsageEvent;

        public event RoutedEventHandler Usage
        {
            add { AddHandler(UsageEvent, value); }
            remove { RemoveHandler(UsageEvent, value); }
        }

        protected virtual void OnUsage()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = UsageEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent NLWEvent;

        public event RoutedEventHandler NLW
        {
            add { AddHandler(NLWEvent, value); }
            remove { RemoveHandler(NLWEvent, value); }
        }

        protected virtual void OnNLW()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = NLWEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent DeriveAnswerEvent;

        public event RoutedEventHandler DeriveAnswer
        {
            add { AddHandler(DeriveAnswerEvent, value); }
            remove { RemoveHandler(DeriveAnswerEvent, value); }
        }

        protected virtual void OnDeriveAnswer()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = DeriveAnswerEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent DeriveQuestionEvent;

        public event RoutedEventHandler DeriveQuestion
        {
            add { AddHandler(DeriveQuestionEvent, value); }
            remove { RemoveHandler(DeriveQuestionEvent, value); }
        }

        protected virtual void OnDeriveQuestion()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = DeriveQuestionEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent CreateAnswerEvent;

        public event RoutedEventHandler CreateAnswer
        {
            add { AddHandler(CreateAnswerEvent, value); }
            remove { RemoveHandler(CreateAnswerEvent, value); }
        }

        protected virtual void OnCreateAnswer()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CreateAnswerEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent PropertiesEvent;

        public event RoutedEventHandler Properties
        {
            add { AddHandler(PropertiesEvent, value); }
            remove { RemoveHandler(PropertiesEvent, value); }
        }

        protected virtual void OnProperties()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = PropertiesEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent CancelEvent;

        public event RoutedEventHandler Cancel
        {
            add { AddHandler(CancelEvent, value); }
            remove { RemoveHandler(CancelEvent, value); }
        }

        protected virtual void OnCancel()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CancelEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent SaveEvent;

        public event RoutedEventHandler Save
        {
            add { AddHandler(SaveEvent, value); }
            remove { RemoveHandler(SaveEvent, value); }
        }

        protected virtual void OnSave()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = SaveEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent FindEvent;

        public event RoutedEventHandler Find
        {
            add { AddHandler(FindEvent, value); }
            remove { RemoveHandler(FindEvent, value); }
        }

        protected virtual void OnFind()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = FindEvent;
            RaiseEvent(args);
        }

        public static RoutedEvent AuditEvent;

        public event RoutedEventHandler Audit
        {
            add { AddHandler(AuditEvent, value); }
            remove { RemoveHandler(AuditEvent, value); }
        }

        protected virtual void OnAudit()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = AuditEvent;
            RaiseEvent(args);
        }

        #endregion
    }
}
