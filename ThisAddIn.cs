﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using System.IO;
using System.Web;
using System.Net;
using System.Xml;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;


/*TODO:

    Check what happens:
    - Tracking revisions
    - Ctrl+Z
    - Copy & paste
    - opening, saving, auto-saving documents
    
    - checking in background
    - license
    - map language codes from MS Word to ISO codes

  DONE:
      - get language names and codes from server 

*/

namespace languagetool_msword10_addin
{
    public partial class ThisAddIn
    {
        private readonly int maxSuggestions = 10;
        
        Word.Application application;
        private string[] comandBarNames = new string[] { "Text", "Footnotes", "Lists" };

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            application = this.Application;
            application.CustomizationContext = application.ActiveDocument;
            application.WindowBeforeRightClick +=
                new Word.ApplicationEvents4_WindowBeforeRightClickEventHandler(application_WindowBeforeRightClick);
            application.DocumentBeforeSave += new Word.ApplicationEvents4_DocumentBeforeSaveEventHandler(application_DocumentBeforeSave);
            application.WindowSelectionChange += new Word.ApplicationEvents4_WindowSelectionChangeEventHandler(application_SelectionChange);
            application.DocumentOpen += new Word.ApplicationEvents4_DocumentOpenEventHandler(application_DocumenOpen);
            
            hookId = SetHook(procedure);

        }

        private void application_DocumenOpen(Word.Document Doc)
        {
            //checkActiveDocument(); //do it in background
            /*var thread = new Thread(() =>
            {
                checkActiveDocument();
            });
            thread.Start();*/
        }

        private void application_SelectionChange(Selection sel)
        {
            if (!sel.Range.GrammarChecked)
            {
                checkParagraphsInSelection();
            }
        }

        private void application_DocumentBeforeSave(Word.Document Doc, ref bool SaveAsUI, ref bool Cancel)
        {
            removeAllErrorMarks();  // Also in auto-saving?
        }

        public void application_WindowBeforeRightClick(Word.Selection selection, ref bool Cancel)
        {
 
            if (selection != null && !String.IsNullOrEmpty(selection.Text))
            {
                string selectionText = selection.Text;
                //if (selection.Font.Underline == WdUnderline.wdUnderlineWavy)
                if (selection.Range.HighlightColorIndex == WdColorIndex.wdTurquoise ||
                    selection.Range.HighlightColorIndex == WdColorIndex.wdBrightGreen ||
                    selection.Range.HighlightColorIndex == WdColorIndex.wdPink 
                    )
                {
                    Regex regex = new Regex("\\[(.*)\\|(.*)\\|(.*)\\]");
                    Match match = regex.Match(findHiddenData(selection));
                    if (match.Success)
                    {
                        String errorStr = match.Groups[3].Value;
                        String[] suggestions = match.Groups[2].Value.Split('#');
                        foreach (String comandBarName in comandBarNames)
                        {
                            Office.CommandBar commandBar = application.CommandBars[comandBarName];
                            commandBar.Reset();

                            // message button
                            Office.CommandBarButton button1 = (Office.CommandBarButton)commandBar.Controls.Add(Office.MsoControlType.msoControlButton, 1, "info_error", 1, true);
                            button1.Tag = "LTMessage";
                            button1.Caption = match.Groups[1].Value;
                            button1.Enabled = false;
                            button1.Picture = getImage();
                            
                            //replacement buttons
                            if (!string.IsNullOrWhiteSpace(suggestions[0]))
                            {
                                int i = 0;
                                while (i<suggestions.Length && i< maxSuggestions) { 
                                    Office.CommandBarButton button2 = (Office.CommandBarButton)commandBar.Controls.Add(Office.MsoControlType.msoControlButton, 1, errorStr, i+2, true);
                                    button2.Tag = "LTSuggestion" + i;
                                    button2.Caption = suggestions[i];
                                    button2.Click +=  new Office._CommandBarButtonEvents_ClickEventHandler(LTbutton_Click);
                                    i++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // remove buttons in command bars
                    foreach (String comandBarName in comandBarNames)
                    {
                        Office.CommandBar commandBar = application.CommandBars[comandBarName];
                        commandBar.Reset();
                    }
                }

            }
        }

        public void LTbutton_Click(Office.CommandBarButton ctrl, ref bool cancel)
        {
            if (ctrl == null)
            {
                return;
            }
            //Select underlined words and replace with selected suggestion
            Word.Range rng = Globals.ThisAddIn.Application.Selection.Range;

            int currenSelectionStart = rng.Start;
            int currentSelectionEnd = rng.End;

            //Word.Range rng = selection.Range;
            object findText = Type.Missing; object matchCase = Type.Missing; object matchWholeWord = Type.Missing; object matchWildCards = Type.Missing; object matchSoundsLike = Type.Missing;
            object matchAllWordForms = Type.Missing; object forward = Type.Missing; object wrap = Type.Missing; object format = Type.Missing; object replaceWithText = Type.Missing;
            object replace = Type.Missing; object matchKashida = Type.Missing; object matchDiacritics = Type.Missing; object matchAlefHamza = Type.Missing; object matchControl = Type.Missing;

            wrap = WdFindWrap.wdFindStop;

            rng.Find.ClearFormatting();
            rng.Find.Font.Hidden = 0;
            //rng.Find.Font.Underline = WdUnderline.wdUnderlineWavy;
            rng.Find.Highlight = 1;

            // move forward to find the end of the error
            forward = true;
            rng.Find.Execute(ref findText, ref matchCase, ref matchWholeWord, ref matchWildCards,
                ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText,
                ref replace, ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);
            int rangeEnd = rng.End;

            // move backward to find the start of the error
            forward = false;
            rng.Find.Execute(ref findText, ref matchCase, ref matchWholeWord, ref matchWildCards,
                ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText,
                ref replace, ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);
            int rangeStart = rng.Start;
           
            //replace the error with the suggestion 
            rng.End = rangeEnd;
            rng.Start = rangeStart;
            String errorToReplace = ctrl.Parameter.ToString();
            String textToSearch = rng.Text;
            if (string.IsNullOrWhiteSpace(errorToReplace) || string.IsNullOrWhiteSpace(textToSearch))
                return;
            int indexFound = textToSearch.IndexOf(errorToReplace);
            if (indexFound >= 0)
            {
                rng.Start += indexFound;
                rng.End = rng.Start + errorToReplace.Length;
                rng.Text = ctrl.Caption;
                //rng.Font.Underline = WdUnderline.wdUnderlineNone;
                rng.HighlightColorIndex = WdColorIndex.wdNoHighlight;
                //rng.Paragraphs.First.Range.GrammarChecked = false;
            }
            // remove buttons in command bars
            foreach (String comandBarName in comandBarNames)
            {
                Office.CommandBar commandBar = application.CommandBars[comandBarName];
                commandBar.Reset();
            }
        }


        public static void checkParagraphsInSelection()
        {
            //Checks whole paragraphs in the current selection.            
            Microsoft.Office.Interop.Word.Document Doc = Globals.ThisAddIn.Application.ActiveDocument;
            if (Doc == null || Doc.ReadOnly)
            {
                return;
            }           
            Word.Range initRng = Globals.ThisAddIn.Application.Selection.Range;
            initRng.Start = initRng.Paragraphs.First.Range.Start;
            initRng.End = initRng.Paragraphs.Last.Range.End;
            if (initRng.Text.Equals("\u0002 \r"))  // avoid checking empty footnotes
            {
                return;
            }
            checkRange(initRng);
        }

        private static void checkRange(Word.Range rangeToCheck)
        {
            //var thread = new Thread(() =>
            //{
            
            if (!Globals.Ribbons.Ribbon1.checkBox1.Checked)
                return;
            if (string.IsNullOrWhiteSpace(rangeToCheck.Text))
                return;
            if (rangeToCheck.GrammarChecked)
                return;
            Microsoft.Office.Interop.Word.Document Doc = Globals.ThisAddIn.Application.ActiveDocument;
            //Doc.ActiveWindow.View.ShowHiddenText = false;  //peta si el quadre de cerca està obert
            bool isTrackingRevisions = Doc.TrackRevisions;
            Doc.TrackRevisions = false;
            
            removeErrorMarks(rangeToCheck);
            if (string.IsNullOrWhiteSpace(rangeToCheck.Text))
                return;
            Globals.ThisAddIn.Application.ScreenUpdating = false;
            String textToCheck = rangeToCheck.Text.ToString();
            String results = getResultsFromServer(rangeToCheck.LanguageID.ToString(), textToCheck);
            //int myParaOffset = 0; // Not necessary if results are processed in reverse order
            int prevErrorStart = -1;
            int prevErrorEnd = -1;
            int rangeToCheckStart = rangeToCheck.Start;
            List<Dictionary<string, string>> parsedResults = ParseXMLResults(results);
            if (parsedResults == null)
                return;
            foreach (Dictionary<string, string> myerror in parsedResults.Reverse<Dictionary<string, string>>())  
            {
                //Select error start and end
                int offset = int.Parse(myerror["offset"]);
                int errorlength = int.Parse(myerror["errorlength"]);
                string errorStr = myerror["context"].Substring(int.Parse(myerror["contextoffset"]), errorlength);
                if (errorStr.Equals(",*") || errorStr.Equals(";*")) //avoid errors in footnote references
                    continue;
                int errorStart = rangeToCheckStart + offset;// + myParaOffset;
                int errorEnd = errorStart + errorlength;
                // Mark just one error at the same place and avoid overlaping errors
                if (prevErrorEnd > -1 && errorEnd >= prevErrorStart)  
                {
                    continue;
                }
                Word.Range rng = rangeToCheck.Duplicate;
                rng.Start = errorStart;
                rng.End = errorEnd;
                // choose color for underline
                //Word.WdColor mycolor = Word.WdColor.wdColorBlue;
                Word.WdColorIndex myColorIndex = WdColorIndex.wdTurquoise;
                switch (myerror["locqualityissuetype"])
                {
                    case "misspelling":
                        //mycolor = Word.WdColor.wdColorRed;
                        myColorIndex = WdColorIndex.wdPink;
                        break;
                    case "style":
                    case "locale-violation":
                        //mycolor = Word.WdColor.wdColorGreen;
                        myColorIndex = WdColorIndex.wdBrightGreen;
                        break;
                }            
                // unerline errors
                //rng.Font.Underline = WdUnderline.wdUnderlineWavy;
                //rng.Font.UnderlineColor = mycolor;
                rng.HighlightColorIndex = myColorIndex;
                // add hidden data after error. Format: [<error message>|replacement1#replacement2#replacement3...|<error string>]
                string errorData = "[" + myerror["msg"] + "|" + myerror["replacements"] + "|" + errorStr + "]";
                //myParaOffset += errorData.Length;
                rng.Start = errorEnd;
                rng.Text = errorData;
                rng.Font.Hidden = 1;
                //rng.Font.Color = WdColor.wdColorRed;
                //Store previous start and end values
                prevErrorEnd = errorEnd;
                prevErrorStart = errorStart;
                // Track revisions again
            }
            rangeToCheck.GrammarChecked = true;   //TODO: No funciona quan es revisa tot el document
            Doc.TrackRevisions = isTrackingRevisions;
            Globals.ThisAddIn.Application.ScreenUpdating = true;
            //});
            //thread.Start();
        }

        //Checks the whole document including footnotes
        public static void checkActiveDocument()
        {
            Microsoft.Office.Interop.Word.Document Doc = Globals.ThisAddIn.Application.ActiveDocument;
            if (Doc == null || Doc.ReadOnly || !Globals.Ribbons.Ribbon1.checkBox1.Checked)
            {
                return;
            }
            //WaitForm myWaitForm = new WaitForm();
            //myWaitForm.ShowDialog();
            //checks the whole document by paragraphs
            var thread = new Thread(() =>
            {
                //check footnotes
                for (int i = 0; i < Doc.Footnotes.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(Doc.Footnotes[i + Doc.Footnotes.StartingNumber].Range.Text))
                    {
                        checkRange(Doc.Footnotes[i + Doc.Footnotes.StartingNumber].Range);
                        Doc.Footnotes[i + Doc.Footnotes.StartingNumber].Range.GrammarChecked = true;
                    }
                }
                //checkRange(Doc.Content);
                //TODO: find a better way to divide the document in larger parts
                Word.Paragraph firstPara = Doc.Paragraphs.First;
                int numParagraphs = Doc.Paragraphs.Count;
                for (int i = 1; i <= numParagraphs; i++)
                {
                    Word.Paragraph para = firstPara.Next(i - 1);
                    Word.Range myrange = para.Range;
                    if (!string.IsNullOrWhiteSpace(myrange.Text.ToString()))
                    {
                        checkRange(myrange);
                    }
                    myrange.GrammarChecked = true;
                }
            });
            thread.Start();
            //myWaitForm.setMessage("Revisió acabada!");
            //myWaitForm.ShowDialog();

        }

        private String findHiddenData(Word.Selection selection)
        {
            //Retrieve hidden data after underlined words.
            Microsoft.Office.Interop.Word.Document Doc = Globals.ThisAddIn.Application.ActiveDocument;
            if (Doc == null || Doc.ReadOnly)
            {
                return "";
            }

            object findText = "(\\[*\\])";
            object matchCase = false;
            object matchWholeWord = false;
            object matchWildCards = true;
            object matchSoundsLike = false;
            object matchAllWordForms = false;
            object forward = true;
            object wrap = WdFindWrap.wdFindStop;
            object format = true;
            object replaceWithText = "\\1";
            object replace = WdReplace.wdReplaceNone;
            object matchKashida = false;
            object matchDiacritics = false;
            object matchAlefHamza = false;
            object matchControl = false;
                        
            Word.Range rng = selection.Range;                   
            rng.Find.ClearFormatting();
            rng.Find.Font.Hidden = 1;
            rng.Find.Replacement.ClearFormatting();
            rng.Find.Replacement.Font.Hidden = 1;

            Globals.ThisAddIn.Application.ScreenUpdating = false;
            bool isShowingHiddenText = Doc.ActiveWindow.View.ShowHiddenText; //Find & replace work better this way!
            Doc.ActiveWindow.View.ShowHiddenText = true;

            //execute find and replace
            bool found = rng.Find.Execute(ref findText, ref matchCase, ref matchWholeWord,
                ref matchWildCards, ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText, ref replace,
                ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);

            String msg = "";
            if (found && rng.Text!= null)
            {
                msg = rng.Text;
            }
            Doc.ActiveWindow.View.ShowHiddenText = isShowingHiddenText;
            Globals.ThisAddIn.Application.ScreenUpdating = true;
            return msg;
        }

        public static void removeAllErrorMarks()
        {
            //TODO could be quicker with WdFindWrap.wdFindContinue
            Microsoft.Office.Interop.Word.Document Doc = Globals.ThisAddIn.Application.ActiveDocument;
            removeErrorMarks(Doc.Content);
            //check footnotes
            for (int i = 0; i < Doc.Footnotes.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(Doc.Footnotes[i + Doc.Footnotes.StartingNumber].Range.Text))
                {
                    removeErrorMarks(Doc.Footnotes[i + Doc.Footnotes.StartingNumber].Range);
                }
            }

        }

        public static void removeErrorMarks(Word.Range initRng)
        {
            Word.Range rng = initRng.Duplicate;
            if (string.IsNullOrWhiteSpace(rng.Text))
            {
                return;
            }
            Microsoft.Office.Interop.Word.Document Doc = Globals.ThisAddIn.Application.ActiveDocument;
            if (Doc == null || Doc.ReadOnly)
            {
                return;
            }
            bool isTrackingRevisions = Doc.TrackRevisions;
            Doc.TrackRevisions = false;

            rng.HighlightColorIndex = WdColorIndex.wdNoHighlight;

            //options
            object findText = Type.Missing;
            object replaceWithText = Type.Missing; 
            object matchCase = false;
            object matchWholeWord = false;
            object matchWildCards = false;
            object matchSoundsLike = false;
            object matchAllWordForms = false;
            object forward = true;
            object format = true;
            object matchKashida = false;
            object matchDiacritics = false;
            object matchAlefHamza = false;
            object matchControl = false;
            object read_only = false;
            object visible = true;
            object replace = WdReplace.wdReplaceAll;
            object wrap = WdFindWrap.wdFindStop;
            
            /*rng.Find.ClearFormatting();
            rng.Find.Replacement.ClearFormatting();
            rng.Find.Font.Underline = WdUnderline.wdUnderlineWavy;
            rng.Find.Replacement.Font.Underline = WdUnderline.wdUnderlineNone;
            //execute find and replace
            //rng.Font.Underline = WdUnderline.wdUnderlineNone;
            rng.Find.Execute(ref findText, ref matchCase, ref matchWholeWord,
                ref matchWildCards, ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText, ref replace,
                ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);*/
            //Remove hidden data
            findText = "";
            replaceWithText = "";
            //matchWildCards = true;
            replace = WdReplace.wdReplaceAll;
            wrap = WdFindWrap.wdFindStop;
            rng.Find.ClearFormatting();
            rng.Find.Replacement.ClearFormatting();
            rng.Find.Font.Hidden = 1;
            //execute find and replace
            Globals.ThisAddIn.Application.ScreenUpdating = false; 
            Doc.ActiveWindow.View.ShowHiddenText = true;  //Find & replace work better this way!
            rng.Find.Execute(ref findText, ref matchCase, ref matchWholeWord,
                ref matchWildCards, ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText, ref replace,
                ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);
            Doc.ActiveWindow.View.ShowHiddenText = false;
            Globals.ThisAddIn.Application.ScreenUpdating = true;
            Doc.TrackRevisions = isTrackingRevisions;
            rng.GrammarChecked = false;
        }
        
        private static List<Dictionary<string, string>> ParseXMLResults(String xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
                return null;
            XElement xml = XElement.Parse(xmlString);
            var suggestions = new List<Dictionary<string, string>>();

            foreach (var myerror in xml.Descendants("error"))
            {
                var suggestion = new Dictionary<string, string>();
                foreach (var myattribute in myerror.Attributes())
                {
                    suggestion.Add(myattribute.Name.ToString(), myattribute.Value);
                }
                suggestions.Add(suggestion);
            }
            return suggestions;
        }

        private static string getLanguageCode(string langID)
        {
            if (langID.StartsWith("wdSpanish"))
                return "es";
            switch (langID)
            {
                case "wdCatalan":
                    if (Properties.Settings.Default.CatalanUserPreferences.StartsWith("valencià"))
                        return "ca-ES-valencia";
                    else
                        return "ca-ES";
                case "wdEnglishUS":
                    return "en-US";
                case "wdEnglishUK":
                    return "en-UK";
                case "wdFrench":
                    return "fr";
                case "wdGerman":
                    return "de-DE";
                case "wdItalian":
                    return "it";
                case "wdPolish":
                    return "pl-PL";
                case "wdByelorussian":
                    return "be";
                case "wdPortuguese":
                    return "pt-PT";
            }
            return (Properties.Settings.Default.DefaultLanguage);
        }

        private static string getUrlParameters(string langID)
        {
            string enabledRules = "";
            string disabledRules = "";
            string urlParameters = "";
            if (langID.Equals("wdCatalan")) {
                switch (Properties.Settings.Default.CatalanUserPreferences)
                {
                    case "general":
                        enabledRules += ",EXIGEIX_PLURALS_S";
                        break;
                    case "valencià":
                        break;
                    case "valencià (accentuació general)":
                        disabledRules += ",EXIGEIX_ACCENTUACIO_VALENCIANA";
                        enabledRules += ",EXIGEIX_ACCENTUACIO_GENERAL";
                        break;
                    case "balear":
                        enabledRules += ",EXIGEIX_VERBS_BALEARS";
                        disabledRules += ",EXIGEIX_VERBS_CENTRAL";
                        break;
                }
                if (Properties.Settings.Default.TypographyRulesEnabled)
                {
                    enabledRules += ",PRIORITZAR_COMETES,GUIONET_GUIO,COMETES_TIPOGRAFIQUES," 
                        +" GUIO_SENSE_ESPAI,APOSTROF_TIPOGRAFIC,PUNTS_SUSPENSIUS,EVITA_EXCLAMACIO_INICIAL";
                }
            }
            if (disabledRules.Length > 0)
                urlParameters += "&disabled=" + disabledRules;
            if (enabledRules.Length > 0)
                urlParameters += "&enabled=" + enabledRules;
            return urlParameters;
        }
        private static string getResultsFromServer(string langID, string textToCheck)
        {
            if (string.IsNullOrWhiteSpace(textToCheck)) {
                return "";
            }

            textToCheck = textToCheck.Replace("\u0002", "*"); //char used for footnote references 
            string uriString = Properties.Settings.Default.LTServer + "?language=" + getLanguageCode(langID) 
                + "&text=" + WebUtility.UrlEncode(textToCheck) + getUrlParameters(langID);
            uriString = uriString.Replace("%C2%A0", "+"); // replace non-breaking space. Why?
            Uri uri = new Uri(uriString); //TODO set a limit of length
            string result = "";
            try
            {
                // Create the web request  
                System.Net.HttpWebRequest request = System.Net.WebRequest.Create(uri) 
                    as System.Net.HttpWebRequest;
                // Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream(), 
                        System.Text.Encoding.UTF8);
                    // Read the whole contents and return as a string  
                    result = reader.ReadToEnd();
                }
                return result;
            }
            catch 
            {
                System.Windows.Forms.MessageBox.Show("No es pot contactar amb el servidor: " 
                    + Properties.Settings.Default.LTServer + ".");
            }
            return "";
        }

        public static Dictionary<string, string> getLanguagesFromServer()
        {
            string xmlResults = "";
            string uri = Properties.Settings.Default.LTServer + "Languages";
            try
            {
                // Create the web request  
                System.Net.HttpWebRequest request = System.Net.WebRequest.Create(uri)
                    as System.Net.HttpWebRequest;
                // Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream(),
                        System.Text.Encoding.UTF8);
                    // Read the whole contents and return as a string  
                    xmlResults = reader.ReadToEnd();
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("No es pot contactar amb el servidor: "
                    + Properties.Settings.Default.LTServer + ".");
            }

            if (string.IsNullOrWhiteSpace(xmlResults))
                return null;
            XElement xml = XElement.Parse(xmlResults);
            var languages = new Dictionary<string, string>();
            foreach (var lang in xml.Descendants("language"))
            {
                languages.Add(lang.Attribute("name").Value, lang.Attribute("abbrWithVariant").Value);
            }
            return languages;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static IntPtr hookId = IntPtr.Zero;
        private delegate IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam);
        private static HookProcedure procedure = HookCallback;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProcedure lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr SetHook(HookProcedure procedure)
        {
            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
                return SetWindowsHookEx(WH_KEYBOARD_LL, procedure, GetModuleHandle(module.ModuleName), 0);
        }


        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int pointerCode = Marshal.ReadInt32(lParam);
                if (pointerCode == 162 || pointerCode == 160)
                {
                    return CallNextHookEx(hookId, nCode, wParam, lParam);
                }
                string pressedKey = ((Keys)pointerCode).ToString();
                //Do some sort of processing on key press
                    Globals.ThisAddIn.application.CustomizationContext = Globals.ThisAddIn.application.ActiveDocument;
                    Word.Range initRng = Globals.ThisAddIn.Application.Selection.Range;
                    //do something with current document
                    if (initRng != null)
                    {
                        Paragraph para = initRng.Paragraphs.First;
                        Paragraph previousPara = initRng.Paragraphs.First.Previous(0);
                        switch (pressedKey)
                        {
                            case "Return":
                            case "Down":
                                if (previousPara != null)
                                    ThisAddIn.checkRange(previousPara.Range);
                                break;
                            case "Up":
                                Paragraph nextPara = initRng.Paragraphs.First.Next(0);
                                if (nextPara != null)
                                    ThisAddIn.checkRange(nextPara.Range);
                                break;
                            case "Left":
                            case "Right":
                            case "OemPeriod":
                            case "Oemcomma":
                                if (para != null)
                                    ThisAddIn.checkRange(para.Range);
                                break;
                            //case "A" etc...
                            default:
                                //initRng.Paragraphs.First.Range.GrammarChecked = false;
                                break;
                        }
                    }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
        
        sealed public class ConvertImage : System.Windows.Forms.AxHost
        {
            private ConvertImage()
                : base(null)
            {
            }

            public static stdole.IPictureDisp Convert
                (System.Drawing.Image image)
            {
                return (stdole.IPictureDisp)System.
                    Windows.Forms.AxHost
                    .GetIPictureDispFromPicture(image);
            }
        }
        private stdole.IPictureDisp getImage()
        {
            stdole.IPictureDisp tempImage = null;
            try
            {
                System.Drawing.Icon newIcon =
                    Properties.Resources.LanguageTool_Logo;

                System.Windows.Forms.ImageList newImageList =
                    new System.Windows.Forms.ImageList();
                newImageList.Images.Add(newIcon);
                tempImage = ConvertImage.Convert(newImageList.Images[0]);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            return tempImage;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            UnhookWindowsHookEx(hookId);
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
