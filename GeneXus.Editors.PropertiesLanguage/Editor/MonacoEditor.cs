using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Commands;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Common.Framework.Commands;
using Artech.Common.Helpers.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChromiumWebBrowser = Artech.FrameworkDE.WebUtils.GXChromiumBrowser;

namespace GeneXus.Editors.DesignSystemTokens
{
	public abstract partial class MonacoEditor : UserControl, ICommandTarget
	{
		public ChromiumWebBrowser chromeBrowser;

		public event EventHandler DocumentChanged;

		public MonacoEditor(string language)
		{
			InitializeComponent();
			chromeBrowser = new ChromiumWebBrowser();
			if (HiddenFeatureFlag.ShowChromiumDevTools)
				chromeBrowser.AddShowDevToolsMenu = true;

			Controls.Add(chromeBrowser);
			chromeBrowser.Dock = DockStyle.Fill;
			string monacoEditorURL = "";
			if (language.StartsWith("gx-file://"))
				monacoEditorURL = language;
			else
				monacoEditorURL = Path.Combine(PathHelper.StartupPath, "Monaco", $"{language}.html");

			chromeBrowser.ObjectForScripting = GetObjectForScripting();
			chromeBrowser.LoadUrl(monacoEditorURL, null, "", false);
		}

		public abstract object GetObjectForScripting();

		public void OnDocumentChanged()
		{
			DocumentChanged?.Invoke(this, new EventArgs());

		}

		/// <summary>
		/// Set's the Text of Monaco to the Parameter text.
		/// </summary>
		/// <param name="text"></param>
		public void SetText(string text)
		{
			if ((this.chromeBrowser != null))
			{
				chromeBrowser.ExecuteMethodAsync("SetText", text);
			}
		}

		/// <summary>
		/// Get's the Text of Monaco and returns it.
		/// </summary>
		/// <returns></returns>

		public string GetText()
		{
			if ((this.chromeBrowser != null))
			{
				var value = chromeBrowser.EvaluateCodeAsync("GetText()").Result;
				return value is string ? (string)value : string.Empty;
			}
			else
			{
				return string.Empty;
			}
		}

		public bool Exec(CommandKey cmdKey, CommandData cmdData)
		{
			throw new NotImplementedException();
		}

		public bool QueryState(CommandKey cmdKey, CommandData cmdData, ref CommandStatus status)
		{
			return QueryCommandState(cmdKey, cmdData, ref status);
		}

		protected virtual bool QueryCommandState(CommandKey cmdKey, CommandData commandData, ref CommandStatus status)
		{
			if (cmdKey == CommandKeys.Core.SelectAll ||
				 cmdKey == CommandKeys.Core.Find ||
				 cmdKey == CommandKeys.Core.GoToLine ||
				 cmdKey == CommandKeys.TextEditor.ToggleBookmark ||
				 cmdKey == CommandKeys.Core.Print ||
				 cmdKey == CommandKeys.Core.PrintPreview)
			{
				/*	status.State = CommandState.Enabled;
					return true;*/
			}
			else if (cmdKey == CommandKeys.Core.Undo)
			{
				return true;
			}
			else if (cmdKey == CommandKeys.Core.Redo)
			{
				/*if (m_CurrentDialog == null || !m_CurrentDialog.ContainsFocus)
				{
					status.Enable(m_SyntaxEditor.Document.UndoRedo.CanRedo);
					return true;
				}*/
			}
			else if (cmdKey == CommandKeys.Core.Replace ||
				 cmdKey == CommandKeys.TextEditor.MakeLowercase ||
				 cmdKey == CommandKeys.TextEditor.MakeUppercase ||
				 cmdKey == CommandKeys.Core.Indent ||
				 cmdKey == CommandKeys.Core.Unindent ||
				 cmdKey == CommandKeys.TextEditor.CommentSelection ||
				 cmdKey == CommandKeys.TextEditor.UncommentSelection)
			{
				/*	status.Enable(!ReadOnly);
					return true;
				*/
			}
			else if (cmdKey == CommandKeys.Core.Copy)
			{
				return true;
				/*if (m_CurrentDialog == null || !m_CurrentDialog.ContainsFocus)
				{
					status.Enable(!string.IsNullOrEmpty(m_SyntaxEditor.SelectedView.SelectedText));
					return true;
				}*/
			}
			else if (cmdKey == CommandKeys.Core.Cut)
			{
				return true;
				/*if (m_CurrentDialog == null || !m_CurrentDialog.ContainsFocus)
				{
					status.Enable(!string.IsNullOrEmpty(m_SyntaxEditor.SelectedView.SelectedText) && m_SyntaxEditor.SelectedView.CanDelete);
					return true;
				}*/
			}
			else if (cmdKey == CommandKeys.Core.Delete)
			{
				return true;
				/*if (m_CurrentDialog == null || !m_CurrentDialog.ContainsFocus)
				{
					status.Enable(m_SyntaxEditor.SelectedView.CanDelete);
					return true;
				}*/
			}
			else if (cmdKey == CommandKeys.Core.Paste)
			{
				return true;
				/*if (m_CurrentDialog == null || !m_CurrentDialog.ContainsFocus)
				{
					status.Enable(m_SyntaxEditor.SelectedView.CanPaste);
					return true;
				}*/
			}
			/*
			else if (cmdKey == CommandKeys.Core.FontName)
			{
				status.Value = m_SyntaxEditor.Font.Name;
				status.State = CommandState.Enabled;
				return true;
			}
			else if (cmdKey == CommandKeys.Core.FontSize)
			{
				status.Value = FontSizeIntToObject(m_SyntaxEditor.Font.Size);
				status.State = CommandState.Enabled;
				return true;
			}*/
			else if ((cmdKey == CommandKeys.Core.FindNext || cmdKey == CommandKeys.Core.FindPrevious))
			{
				/*if (m_FindReplaceOptions != null && m_FindReplaceOptions.FindText != String.Empty)
				{
					status.State = CommandState.Enabled;
					return true;
				}*/
			}
			else if ((cmdKey == CommandKeys.Core.FindPreviousSelection || cmdKey == CommandKeys.Core.FindNextSelection))
			{
				/*if (m_SyntaxEditor.SelectedView.SelectedText != String.Empty)
				{
					status.State = CommandState.Enabled;
					return true;
				}*/
			}
			else if (cmdKey == CommandKeys.TextEditor.CollapseAll || cmdKey == CommandKeys.TextEditor.ExpandAll)
			{
				/*if (m_SyntaxEditor.Document.Outlining.Mode == OutliningMode.Automatic)
					status.State = CommandState.Enabled;
				else status.State = CommandState.Invisible;
				return true;
				*/
			}
			return false;
		}

		/// <summary>
		/// Editor Text
		/// </summary>
		public override string Text
		{
			get
			{
				return GetText();
			}
			set
			{
				SetText(value);
			}
		}
	}

	// Esto lo sacamos hacia la integracion del editor
	public abstract class GxMonacoEditor
		: MonacoEditor, IGxView
	{
		private IViewed m_Document;
		private bool m_Readonly;
		private string Source;

		public GxMonacoEditor(string language)
			: base(language)
		{
		}



		public IViewed Viewed
		{
			get => m_Document;
			set
			{
				m_Document = value;
				Debug.Assert(value is IGxDocumentPart, "Document in TextEditor must implement IGxDocumentPart");
				m_Document = value;
				this.DidSetDocument();

				GxDocumentPart.GetDocument().BeforeSaveDocument += TextEditor_BeforeSaveDocument;
				GxDocumentPart.GetDocument().AfterSaveDocument += TextEditor_AfterSaveDocument;
				ISource data = m_Document.GetData() as ISource;
				Source = data == null ? string.Empty : data.Source;
				SetText(Source);
				DocumentChanged += WebUserControlEditor_DocumentChanged;
			}
		}

		private void DidSetDocument()
		{
			var documentParent = (this.Viewed as IGxDocumentPart).GetDocument();
			var documentObject = documentParent.Object;
			(this.chromeBrowser.ObjectForScripting as IDocumentHost).SetDocumentObject(documentObject);
		}

		private void WebUserControlEditor_DocumentChanged(object sender, EventArgs e)
		{
			if (!m_bIgnoreChanges)
			{
				Source = Text;
				OnDocumentDataChanged(Source);
			}
		}

		private void TextEditor_AfterSaveDocument(object sender, DocumentEventArgs e)
		{

		}

		private void TextEditor_BeforeSaveDocument(object sender, DocumentCancelEventArgs e)
		{

		}

		private IGxDocumentPart GxDocumentPart
		{
			get { return m_Document as IGxDocumentPart; }
		}

		public bool ReadOnly { get => m_Readonly; set => m_Readonly = value; }

		public int Count => 0;

		public object SelectedObject => null;

		public ICollection SelectedObjects => null;

		public ICollection ObjectsToSave => null;

		public bool KeepSelection => true;

		public object Context
		{
			get
			{
				return GxDocumentPart?.GetDocument();
			}
		}

		public event DataChangedEventHandler DocumentDataChanged;
		public event EventHandler SelectionChanged;

		public void OnSelectionChanged()
		{
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OnDocumentDataChanged(object data)
		{
			DocumentDataChanged?.Invoke(this, data);
		}

		public event DataSavedEventHandler DocumentDataSaved;
		private void OnDocumentSaved(object data)
		{
			DocumentDataSaved?.Invoke(this, data);
		}

		public void Activate()
		{

		}

		public void BeforeSaveDocument()
		{
		}

		public void Initialize(OpenInformation info)
		{

		}

		public void Release()
		{

		}

		public void UpdateData()
		{
			SaveDocument();
		}

		private bool m_bIgnoreChanges = false;
		private void SaveDocument()
		{
			try
			{
				if (GxDocumentPart.Dirty)
				{
					((ISource)GxDocumentPart.Part).Source = Source;
					OnDocumentSaved(Source);
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError("Artech.Packages.Editors.Text.TextEditor.SaveDocument() exception: " + ex.Message);
				Debug.Assert(false);
			}
		}


		public void UpdateView()
		{
			if (!(m_Document.GetData() is ISource data))
			{
				Text = string.Empty;
			}
			else
			{
				Text = data.Source;
			}
		}

		public override object GetObjectForScripting()
		{
			return new DesignSystemPartHost<KBObjectPart>(this, null);
		}
	}

	// Esto lo sacamos hacia la integracion del editor

	public class WebUserControlEditor : GxMonacoEditor
	{
		public WebUserControlEditor()
			: base("UserControl")
		{

		}

		public override object GetObjectForScripting()
		{
			return new UserControlHost(this, null);
		}
	}
}
