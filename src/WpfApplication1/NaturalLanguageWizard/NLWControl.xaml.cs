using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace NaturalLanguageWizard
{
    /// <summary>
    /// Interaction logic for NLWControl.xaml
    /// </summary>
    public partial class NLWControl : UserControl
    {
        #region Constructor


        public NLWControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Set Up


		public ABRichTextBox TxtInsert
		{
			get { return this.txtInsert; }
			set { this.txtInsert = value; }
		}

        private IEnumerable<Algo> algos = null;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void SetUpTextPredictors()
        {
            SetUpHierarchies();

            SetUpAlgoList();
			
            SetUpQuestionTextPredictor();

            SetUpPairAlgoList();

            SetUpPairQuestionTextPredictor();

            SetUpConclusionAlgoList();

            SetUpConclusionTextPredictor();

			this.txtInsert.Focus();
			
        }

        private void SetUpHierarchies()
        {
            tpAlgo.Child = tpQuestion;
            tpAlgo.GetChildren = AssetReader.GetQuestions;

            tpPairAlgo.Child = tpPairQuestion;
            tpPairAlgo.GetChildren = AssetReader.GetQuestions;

            tpConclusionAlgo.Child = tpConclusion;
            tpConclusionAlgo.GetChildren = AssetReader.GetConclusions;
        }

        private void SetUpAlgoList()
        {
            if(algos == null) algos = AssetReader.GetAlgos();
            tpAlgo.DataSource = algos;
        }

        private void SetUpQuestionTextPredictor()
        {
            tpQuestion.TryGetAsset = AssetReader.TryGetQuestion;
            tpQuestion.AssetSelected += new TextPredictor.AssetSelectedHandler(tpQuestion_AssetSelected);
        }

        private void SetUpPairAlgoList()
        {
            if (algos == null) algos = AssetReader.GetAlgos();
            tpPairAlgo.DataSource = algos;
        }

        private void SetUpPairQuestionTextPredictor()
        {
            tpPairQuestion.TryGetAsset = AssetReader.TryGetQuestion;
            tpPairQuestion.AssetSelected += new TextPredictor.AssetSelectedHandler(tpPairQuestion_AssetSelected);
        }

        private void SetUpConclusionAlgoList()
        {
            if (algos == null) algos = AssetReader.GetAlgos();
            tpConclusionAlgo.DataSource = algos;
        }

        private void SetUpConclusionTextPredictor()
        {
            tpConclusion.TryGetAsset = AssetReader.TryGetConclusion;
            tpConclusion.AssetSelected += new TextPredictor.AssetSelectedHandler(tpConclusion_AssetSelected);
        }

        #endregion

        #region General

        public string GetSelectedRadioButtonTag(params SaveableRadioButton[] buttons)
        {
            foreach (var button in buttons)
            {
                if (button.IsChecked.Value)
                {
                    return button.Tag.ToString();
                }
            }

            return null;
        }

        public string GetSelectedRadioButtonTag(params RadioButton[] buttons)
        {
            foreach (var button in buttons)
            {
                if (button.IsChecked.Value)
                {
                    return button.Tag.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Inserts text into txtInsert at current cursor position,
        /// then shifts cursor to end of this text
        /// </summary>
        /// <param name="text"></param>
        private void InsertText(string text)
        {
            InsertText(text, text.Length);
        }

        /// <summary>
        /// Inserts text into txtInsert at current cursor position,
        /// then shifts cursor by the offset amount
        /// </summary>
        /// <param name="text"></param>
        /// <param name="offset"></param>
        private void InsertText(string text, int offset)
        {
			
            // for now, ignore offset
            //offset = text.Length;

			
			//var insertionPoint = txtInsert.CaretPosition.GetOffsetToPosition(this.txtInsert.CaretPosition);
            //txtInsert.Text = txtInsert.Text.Insert(insertionPoint, text);
			//txtInsert.CaretPosition = this.txtInsert.Document.ContentEnd;
            txtInsert.CaretPosition.InsertTextInRun(text);

			TextPointer m = this.txtInsert.CaretPosition.GetPositionAtOffset(-1, LogicalDirection.Forward);
			
			


			System.Diagnostics.Debug.WriteLine(this.txtInsert.CaretPosition.GetTextInRun(LogicalDirection.Forward));

			this.txtInsert.Focus();
			this.txtInsert.CaretPosition = m;
            ///txtInsert.CaretIndex = insertionPoint + offset;
        }

        /// <summary>
        /// Selects all text in a textbox
        /// </summary>
        /// <param name="textbox"></param>
        private void SelectAllTextbox(TextBox textbox)
        {
            textbox.SelectAll();
        }

        #region Events

        public static RoutedEvent InsertNLWEvent;

        public event RoutedEventHandler InsertNLW
        {
            add { AddHandler(InsertNLWEvent, value); }
            remove { RemoveHandler(InsertNLWEvent, value); }
        }

        public class InsertNLWEventArgs : RoutedEventArgs
        {
            public string Text { get; set; }
        }

        protected virtual void OnInsertNLW(string text)
        {
            var args = new InsertNLWEventArgs { Text = text };
            args.RoutedEvent = InsertNLWEvent;
            RaiseEvent(args);
        }

        static NLWControl()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(NLWControl), new FrameworkPropertyMetadata(typeof(NLWControl)));

            InsertNLWEvent = EventManager.RegisterRoutedEvent("btnInsert", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(NLWControl));
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            //var doc = txtInsert.Document;

            //var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;

            string text = txtInsert.Text;

            OnInsertNLW(text);
        }

        /// <summary>
        /// Stops user from entering anything but a number in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.IsANumber();
        }

        /// <summary>
        /// Ensures that only one expander can be in expanded state at any given time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exp_Expanded(object sender, RoutedEventArgs e)
        {
            //var exp = sender as Expander;

            //expGender.ExpandOnlyIfSelected(exp);
            //expAgeRange.ExpandOnlyIfSelected(exp);
            //expAnswer.ExpandOnlyIfSelected(exp);
            //expTest.ExpandOnlyIfSelected(exp);
        }

        #endregion

        #endregion

        #region Gender

        #region Events

        private void btnGender_Click(object sender, RoutedEventArgs e)
        {
            var genderChar = GetSelectedRadioButtonTag(rbtnFemales, rbtnMales);

            var text = txtGender.Text;

            InsertText(string.Format("~{0}{1}~", genderChar, text), 2 + text.Length);

            btnGenderRestore.IsEnabled = true;

            txtGender.SaveAndClear();

            //new SaveableRadioButton[] { rbtnFemales, rbtnMales }.SaveAndInitialise(rbtnFemales);
        }

        private void btnGenderRestore_Click(object sender, RoutedEventArgs e)
        {
            txtGender.RestoreLast();

            //new SaveableRadioButton[] { rbtnFemales, rbtnMales }.RestoreLast();
        }

        #endregion

        #endregion

        #region Age Range

        private bool FromAgeIsGreaterThanToAge()
        {
            return int.Parse(txtFrom.Text) > int.Parse(txtTo.Text);
        }

        private bool FromAgeOrToAgeNotFilled()
        {
            return (txtFrom.Text == string.Empty || txtTo.Text == string.Empty);
        }

        private void AgeRangeEntered(TextBox ageTextBox, RoutedEventArgs e)
        {
            if (FromAgeOrToAgeNotFilled())
            {
                btnAgeRange.IsEnabled = false;
            }

            else if (FromAgeIsGreaterThanToAge())
            {
                btnAgeRange.IsEnabled = false;

                MessageBox.Show("Age range is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                e.Handled = true;

                SelectAllTextbox(ageTextBox);
            }

            else
            {
                btnAgeRange.IsEnabled = true;
            }
        }

        #region Events

        private void btnAgeRange_Click(object sender, RoutedEventArgs e)
        {
            var from = string.Format("{0,3:000}", int.Parse(txtFrom.Text));
            var to = string.Format("{0,3:000}", int.Parse(txtTo.Text));

            var period = GetSelectedRadioButtonTag(rbtnDays, rbtnWeeks, rbtnMonths, rbtnYears);

            var text = txtAgeRange.Text;

            InsertText(string.Format("{{{0}-{1}{2}{3}}}", from, to, period, text), 9 + text.Length);

            btnAgeRangeRestore.IsEnabled = true;

            txtAgeRange.SaveAndClear();

            txtFrom.SaveAndClear();

            txtTo.SaveAndClear();

            //new SaveableRadioButton[] { rbtnDays, rbtnWeeks, rbtnMonths, rbtnYears }.SaveAndInitialise(rbtnYears);

            btnAgeRange.IsEnabled = false;
        }

        private void txtAge_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            AgeRangeEntered(sender as TextBox, e);
        }

        private void txtAge_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AgeRangeEntered(sender as TextBox, e);
            }
        }

        private void btnAgeRangeRestore_Click(object sender, RoutedEventArgs e)
        {
            txtAgeRange.RestoreLast();

            txtFrom.RestoreLast();

            txtTo.RestoreLast();

            //new SaveableRadioButton[] { rbtnDays, rbtnWeeks, rbtnMonths, rbtnYears }.RestoreLast();

            btnAgeRange.IsEnabled = true;
        }

        #endregion

        #endregion

        #region Question Answer

        #region Events

        private void btnAnswerRestore_Click(object sender, RoutedEventArgs e)
        {
            tpQuestion.RestoreLast();
        }

        private void tpAlgo_AssetSelected(object sender, TextPredictor.AssetSelectedArgs e)
        {
            tpQuestion.IsFiltered = (e.Asset != null);
        }

        private void tpQuestion_AssetSelected(object sender, TextPredictor.AssetSelectedArgs e)
        {
            if (e.Asset != null)
            {
                int qid = e.Asset.ID;

                var type = (e.Asset as Question).Type;

                // display first letter upper/lower case selector               
                //gpbxFirstLetter.Visibility = (type.IsText() ? Visibility.Visible : Visibility.Hidden);

                // display or/and selector
                gpbxJoin.Visibility = (type == QuestionType.MultipleAnswer ? Visibility.Visible : Visibility.Hidden);

                btnAnswer.IsEnabled = true;
            }

            else
            {
                //gpbxFirstLetter.Visibility = 
                gpbxJoin.Visibility = Visibility.Hidden;

                btnAnswer.IsEnabled = false;
            }
        }

        private void btnAnswer_Click(object sender, RoutedEventArgs e)
        {
            int qid = tpQuestion.SelectedID.Value;

            var question = AssetReader.TryGetQuestion(qid);

            var typeChar = AssetReader.GetQuestionChar(question);

            var type = question.Type;

            var text = string.Format("{{q{0}{1}}}", typeChar, qid);

            // var case = Utils.GetSelectedRadioButtonTag(rbtnUpperCase, rbtnLowerCase);

            if (type == QuestionType.MultipleAnswer)
            {
                var join = GetSelectedRadioButtonTag(rbtnAnd, rbtnOr);

                text = string.Format("{{{0}{1}}}", join, text);
            }

            InsertText(text, text.Length);

            btnAnswerRestore.IsEnabled = true;

            tpQuestion.SaveAndClear();
        }

        #endregion

        #endregion

        #region Tests

        #region Question-Answer pair

        #region Events

        private void tpPairAlgo_AssetSelected(object sender, TextPredictor.AssetSelectedArgs e)
        {
            tpPairQuestion.IsFiltered = (e.Asset != null);
        }

        private void tpPairQuestion_AssetSelected(object sender, TextPredictor.AssetSelectedArgs e)
        {
            tbkPairAnswerDesc.Text = string.Empty;

            if (tpPairQuestion.SelectedID.HasValue)
            {
                cbxPairAnswerID.ItemsSource = AssetReader.GetAnswers(tpPairQuestion.SelectedID.Value);
                cbxPairAnswerID.IsEnabled = true;
            }
            else
            {
                cbxPairAnswerID.ItemsSource = null;
                cbxPairAnswerID.IsEnabled = false;
            }
        }

        private void btnPair_Click(object sender, RoutedEventArgs e)
        {
            var qid = tpPairQuestion.SelectedID.Value;
            var aid = int.Parse(cbxPairAnswerID.Text);

            var reachedText = txtPairReached.Text;
            var notReachedText = txtPairNotReached.Text;

            InsertText(string.Format("{{qp{0}|{1}|{2}|{3}}}", qid, aid, reachedText, notReachedText), 0);

            txtPairReached.SaveAndClear();
            txtPairNotReached.SaveAndClear();

            //cbxPairAnswerID.SaveAndClear();

            tpPairQuestion.SaveAndClear();

            btnPairRestore.IsEnabled = true;

            btnPair.IsEnabled = false;
        }

        private void cbxPairAnswerID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var answer = cbxPairAnswerID.SelectedItem as Answer;

            if (answer == null)
            {
                tbkPairAnswerDesc.Text = string.Empty;

                btnPair.IsEnabled = false;
            }

            else
            {
                tbkPairAnswerDesc.Text = answer.Description;

                btnPair.IsEnabled = true;
            }
        }

        private void btnPairRestore_Click(object sender, RoutedEventArgs e)
        {
            txtPairReached.RestoreLast();
            txtPairNotReached.RestoreLast();

            tpPairQuestion.RestoreLast();

            //cbxPairAnswerID.RestoreLast();
        }

        #endregion

        #endregion

        #region Conclusion

        #region Events

        private void tpConclusionAlgo_AssetSelected(object sender, TextPredictor.AssetSelectedArgs e)
        {
            tpConclusion.IsFiltered = (e.Asset != null);
        }

        private void tpConclusion_AssetSelected(object sender, TextPredictor.AssetSelectedArgs e)
        {
            btnConclusion.IsEnabled = (e.Asset != null);
        }

        private void btnConclusion_Click(object sender, RoutedEventArgs e)
        {
            var cid = tpConclusion.SelectedID.Value;

            var reachedText = txtConclusionReached.Text;
            var notReachedText = txtConclusionNotReached.Text;

            InsertText(string.Format("{{cc{0}|{1}|{2}}}", cid, reachedText, notReachedText), 0);

            tpConclusion.SaveAndClear();

            txtConclusionReached.SaveAndClear();
            txtConclusionNotReached.SaveAndClear();

            btnConclusionRestore.IsEnabled = true;

            btnConclusion.IsEnabled = false;
        }

        private void btnConclusionRestore_Click(object sender, RoutedEventArgs e)
        {
            tpConclusion.RestoreLast();

            txtConclusionReached.RestoreLast();
            txtConclusionNotReached.RestoreLast();
        }

        #endregion

        #endregion

        #endregion
    }
}
