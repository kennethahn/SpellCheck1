using System;
using System.Collections.Generic;
using System.Globalization;
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
            
            textbox.Text = new TextRange(richtextbox.Document.ContentStart, richtextbox.Document.ContentEnd).Text;

            updateLocaleInfo();
        }

        private void localeSelector_Selected(object sender, RoutedEventArgs e)
        {
            if( localeSelector.SelectedItem != null){
                string tmp = (localeSelector.SelectedItem as ComboBoxItem).Content.ToString();

                var culture = CultureInfo.GetCultureInfo(tmp);

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                var lang = System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag);
                //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata( lang ));
                MainWindow.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata( lang ));
                mainWindow.Language = lang;
                grid1.Language = lang;
                richtextbox.Language = lang;
                textbox.Language = lang;
                localeSelector.IsEnabled = false;
                updateLocaleInfo();
            }
        }

        private void updateLocaleInfo()
        {
            if (IsInitialized)
            {
                CultureInfo currentCi = CultureInfo.CurrentCulture;
                CultureInfo currentUICi = CultureInfo.CurrentUICulture;

                threadLocale.Text = currentCi.IetfLanguageTag;
                uilocale.Text = currentUICi.IetfLanguageTag;
                flowdocLocale.Text = richtextbox.Document.Language.IetfLanguageTag;
                //windowLocale.Text = GetWindow(richtextbox).Language.IetfLanguageTag;
                windowLocale.Text = mainWindow.Language.IetfLanguageTag;
                textboxLocale.Text = textbox.Language.IetfLanguageTag;
                var firstErrorPos = richtextbox.GetNextSpellingErrorPosition(richtextbox.Document.ContentStart, LogicalDirection.Forward);
                int i = 0;
                TextPointer pos = richtextbox.GetNextSpellingErrorPosition(richtextbox.Document.ContentStart, LogicalDirection.Forward);
                while (pos != null)
                {
                    var range = richtextbox.GetSpellingErrorRange(pos);
                    pos = range.End;
                    i++;
                    pos = richtextbox.GetNextSpellingErrorPosition(pos, LogicalDirection.Forward);
                }
                errorCount.Content = i;

            }
        }
    }
}
