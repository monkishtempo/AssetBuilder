using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace NaturalLanguageWizard
{
	public partial class TextPredictor : UserControl, ISaveable
	{
		public TextPredictor()
		{
			InitializeComponent();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			textbox.ApplyTemplate();
		}

		#region Private members

		private int _expandedListHeight = 70;
		private bool _isFiltered = true;

		#endregion

		#region Public properties

		public bool IsFiltered
		{
			get
			{
				return _isFiltered;
			}

			set
			{
				_isFiltered = value;

				if (!_isFiltered)
				{
					DataSource = null;

					textbox.IsEnabled = true;
				}

				filter.Visibility = (_isFiltered ? Visibility.Visible : Visibility.Hidden);
			}
		}

		public int ExpandedListHeight
		{
			get { return _expandedListHeight; }
			set { _expandedListHeight = value; }
		}

		public TextPredictor Child { get; set; }

		public GetChildrenDelegate GetChildren { get; set; }

		public TryGetAssetDelegate TryGetAsset { get; set; }

		public IEnumerable DataSource
		{
			get
			{
				return listbox.ItemsSource;
			}

			set
			{
				listbox.ItemsSource = value;

				textbox.IsEnabled = (value != null);

				textbox.Text = string.Empty;

				ClearSelection();
			}
		}

		public int? SelectedID { get; private set; }

		#endregion

		#region Private properties

		private bool ListBoxIsExpanded
		{
			get
			{
				return listbox.Height != 0;
			}
		}

		#endregion

		#region Delegates

		public delegate IEnumerable GetChildrenDelegate(int? parentID);

		public delegate Asset TryGetAssetDelegate(int assetID);

		#endregion

		#region Events

		private void textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var newText = textbox.GetReplacedText(e.Text);

			if (IsFiltered)
			{
				if (!ListHasMatchingItems(newText))
				{
					e.Handled = true;
				}
			}

			else
			{
				e.Handled = !newText.IsANumber();
			}
		}

		private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listbox.SelectedItem != null)
			{
				textbox.Text = listbox.SelectedValue.ToString();

				SelectAsset(listbox.SelectedValue as Asset);
			}
		}

		private void textbox_KeyUp(object sender, KeyEventArgs e)
		{
			if (IsFiltered)
			{
				if (!ListHasMatchingItems(textbox.Text))
				{
					e.Handled = true;
					return;
				}
			}

			ClearSelection();

			if (IsFiltered)
			{
				SetListFilter(textbox.Text);
			}

			if (e.Key == Key.Return)
			{
				CheckEnteredText();
			}
		}

		private void textbox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (!ListBoxItemClicked(e))
			{
				if (CheckEnteredText())
				{
					if (IsFiltered)
					{
						ShrinkList();
					}
				}
				else
				{
					e.Handled = true;
				}
			}
		}

		private void textbox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			// stop user deleting/backspacing into an invalid ID
			if (IsFiltered)
			{
				if (e.Key.IsDeleteOrBack())
				{
					var testText = string.Empty;

					if (textbox.SelectedText != string.Empty)
					{
						testText = textbox.Text.Remove(textbox.SelectionStart, textbox.SelectionLength);
					}
					else
					{
						if ((e.Key.IsDelete() && textbox.HasCaretAtExtremeRight()) ||
							(e.Key.IsBack() && textbox.HasCaretAtExtremeLeft()))
						{
							return;
						}

						else
						{
							var index = (e.Key.IsDelete() ? textbox.CaretIndex : textbox.CaretIndex - 1);

							testText = textbox.Text.Remove(index, 1);
						}
					}

					if (!ListHasMatchingItems(testText))
					{
						e.Handled = true;
						return;
					}
				}
			}
		}
		#endregion

		#region Asset Selected Event

		public delegate void AssetSelectedHandler(object sender, AssetSelectedArgs e);

		public class AssetSelectedArgs : EventArgs
		{
			public Asset Asset { get; set; }
		}

		public event AssetSelectedHandler AssetSelected;

		protected virtual void OnAssetSelected(AssetSelectedArgs e)
		{
			if (AssetSelected != null)
			{
				AssetSelected(this, e);
			}
		}

		#endregion

		protected bool IsMatch(object o, string text)
		{
			return o.ToString().StartsWith(text);
		}

		private Predicate<object> IsMatch(string text)
		{
			return new Predicate<object>(o => IsMatch(o, text));
		}

		private bool ListHasMatchingItems(string text)
		{
			return listbox.ItemsSource.Cast<object>().Any(IsMatch(text).ConvertToFunc());
		}

		private void AnimateList(double seconds, double from, double to)
		{
			var animation = new DoubleAnimation
			{
				Duration = TimeSpan.FromSeconds(seconds),
				From = from,
				To = to
			};

			listbox.BeginAnimation(HeightProperty, animation);
		}

		private void ExpandList()
		{
			AnimateList(0.3, 0, ExpandedListHeight);

			listbox.IsEnabled = true;
			popup.IsOpen = true;

			listbox.SelectedItem = null;
		}

		private void ShrinkList()
		{
			popup.IsOpen = false;
			listbox.IsEnabled = false;

			AnimateList(0, ExpandedListHeight, 0);
		}

		private void SetListFilter(string text)
		{
			listbox.Items.Filter = IsMatch(text);

			listbox.ScrollIntoView(listbox.Items[0]);

			if (text != string.Empty && !ListBoxIsExpanded)
			{
				ExpandList();
			}

			else if (text == string.Empty)
			{
				ShrinkList();
			}
		}

		private void SelectAsset(Asset selectedAsset)
		{
			if (IsFiltered)
			{
				ShrinkList();
			}

			if (selectedAsset != null)
			{
				SetSelection(selectedAsset);
			}

			else
			{
				ClearSelection();
			}
		}

		private void SetSelection(Asset selectedAsset)
		{
			SetSelection(selectedAsset.ID, selectedAsset.Description, selectedAsset);
		}

		private void ClearSelection()
		{
			SetSelection(null, string.Empty, null);
		}

		private void SetSelection(int? id, string description, Asset selectedAsset)
		{
			SelectedID = id;

			tbkDescription.Text = description;
			tbkDescription.ToolTip = new AssetBuilder.Info { Title = string.Format("{0} description", selectedAsset == null ? "Asset" : selectedAsset.AssetType.ToString()), Body = description }.ProvideValue(null);

			if (Child != null)
			{
				Child.DataSource = GetChildren(id);
			}

			OnAssetSelected(new AssetSelectedArgs { Asset = selectedAsset });
		}

		private bool CheckEnteredText()
		{
			var text = textbox.Text;

			if (text == string.Empty)
			{
				SelectAsset(null);
				return true;
			}

			else if (IsFiltered)
			{
				if (listbox.Items.Contains(text))
				{
					var item = listbox.Items[listbox.Items.IndexOf(textbox.Text)];

					SelectAsset(item as Asset);
					return true;
				}

				else
				{
					return false;
				}
			}

			else
			{
				var id = int.Parse(text);
				var asset = TryGetAsset(id);

				if (asset != null)
				{
					SelectAsset(asset);
					return true;
				}

				else
				{
					MessageBox.Show(string.Format("Asset {0} does not exist", id), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					textbox.SelectAll();
					return false;
				}
			}
		}

		private bool ListBoxItemClicked(KeyboardFocusChangedEventArgs e)
		{
			return e.NewFocus is ListBoxItem;
		}

		#region ISaveable Members

		public void SaveAndClear()
		{
			textbox.SaveAndClear();

			ClearSelection();
		}

		#endregion

		#region IRestorable Members

		public void RestoreLast()
		{
			textbox.RestoreLast();

			CheckEnteredText();

			if (Child != null)
			{
				Child.RestoreLast();
			}
		}

		#endregion
	}
}
