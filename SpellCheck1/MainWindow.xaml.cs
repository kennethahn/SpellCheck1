using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpellCheck1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            updateLocaleInfo();
        }

        private void richtextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInitialized && tryfix.IsChecked.HasValue && tryfix.IsChecked.Value )
            {
                #region maybefix
                var changeList = e.Changes.ToList();
                if (changeList.Count > 0)
                {
                    foreach (var change in changeList)
                    {
                        TextPointer start = null;
                        TextPointer end = null;
                        if (change.AddedLength > 0)
                        {
                            start = this.richtextbox.Document.ContentStart.GetPositionAtOffset(change.Offset);
                            end = this.richtextbox.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength);
                        }
                        else
                        {
                            int startOffset = Math.Max(change.Offset - change.RemovedLength, 0);
                            start = this.richtextbox.Document.ContentStart.GetPositionAtOffset(startOffset);
                            end = this.richtextbox.Document.ContentStart.GetPositionAtOffset(change.Offset);
                        }

                        if (start != null && end != null)
                        {
                            var range = new TextRange(start, end);
                            range.ApplyPropertyValue(FrameworkElement.LanguageProperty, richtextbox.Document.Language);
                            
                        }
                    }
                }
                #endregion
            }
            updateLocaleInfo();
        }

        private void localeSelector_Selected(object sender, SelectionChangedEventArgs e)
        {
            if( localeSelector.SelectedItem != null){
                string tmp = (localeSelector.SelectedItem as ComboBoxItem).Content.ToString();

                var culture = CultureInfo.GetCultureInfo(tmp);

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                InputLanguageManager.SetInputLanguage(this, culture);
                var lang = System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag);
                //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata( lang ));
                //localeSelector.IsEnabled = false;
                mainWindow.Language = lang;
                grid1.Language = lang;
                richtextbox.Language = lang;
                richtextbox.Document.Language = lang;
                textbox.Language = lang;
                updateLocaleInfo();
            }
        }

        private void updateLocaleInfo()
        {
            if (IsInitialized)
            {
                CultureInfo currentCi = CultureInfo.CurrentCulture;
                CultureInfo currentUICi = CultureInfo.CurrentUICulture;
                textbox.Clear();
                textbox.AppendText("Thread cult:\t" + currentCi.IetfLanguageTag);
                textbox.AppendText("\nThread UI cult:\t" + currentUICi.IetfLanguageTag);
                textbox.AppendText("\nWindow lang:\t" + mainWindow.Language.IetfLanguageTag);
                textbox.AppendText("\nRichTextBox lang:\t" + richtextbox.Language.IetfLanguageTag);
                textbox.AppendText("\nFlowdoc lang:\t" + richtextbox.Document.Language.IetfLanguageTag);
               
                var tmp = InputLanguageManager.GetInputLanguage(richtextbox);
                tmp = InputLanguageManager.Current.CurrentInputLanguage;
                if (tmp.ThreeLetterISOLanguageName.Equals("ivl"))
                {
                    textbox.AppendText("\nInput lang:\t" + "invariant");
                }
                else
                {
                    textbox.AppendText("\nInput lang:\t" + tmp.IetfLanguageTag);
                }

                var lang = richtextbox.Selection.GetPropertyValue(FrameworkElement.LanguageProperty);
                if (lang == DependencyProperty.UnsetValue)
                {
                    textbox.AppendText("\nSelection lang:\t");
                }
                else
                {
                    textbox.AppendText("\nSelection lang:\t" + lang.ToString());
                }
                 
                var firstErrorPos = richtextbox.GetNextSpellingErrorPosition(richtextbox.Document.ContentStart, LogicalDirection.Forward);
                int i = 0;
                TextPointer pos = richtextbox.GetNextSpellingErrorPosition(richtextbox.Document.ContentStart, LogicalDirection.Forward);
                while (pos != null)
                {
                    var range = richtextbox.GetSpellingErrorRange(pos);
                    pos = range.End;
                    i++;
                    pos = richtextbox.GetNextSpellingErrorPosition(pos, LogicalDirection.Forward);
                    if (i > Byte.MaxValue)
                    {
                        break;
                    }
                }
                textbox.AppendText("\nSpeling errors:\t" + i);


            }
        }

        private void richtextbox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                updateLocaleInfo();
            }
        }
    }
}
