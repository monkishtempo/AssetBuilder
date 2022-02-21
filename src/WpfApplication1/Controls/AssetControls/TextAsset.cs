using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AssetBuilder.AssetControls
{
    class TextAsset : assetControl
    {
        static TextAsset()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextAsset), new FrameworkPropertyMetadata(typeof(TextAsset)));
        }

        public TextAsset()
        {
        }

        TextBox filename;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            filename = GetTemplateChild("txtFilename") as TextBox;
            filename.PreviewTextInput += Filename_PreviewTextInput;
            DataObject.AddPastingHandler(filename, OnFilenamePaste);
            usage.ConcatID = "concat(\"\", \"" + string.Join("\", '\"', \"", usage.AssetID.Split('\"')) + "\")";
        }

        private void OnFilenamePaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetData("Text").ToString().IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1) e.CancelCommand();
        }

        private void Filename_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1) e.Handled = true;
        }

        public TextAsset(XmlNode textasset) : base(textasset)
        {
            assetType = AssetType.TextAsset;
            tableName = "TEXTASSET";
            expert = textasset["Table"]["Filename"];
        }
    }
}
