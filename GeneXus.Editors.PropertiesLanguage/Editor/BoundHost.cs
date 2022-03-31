using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Location;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.Location;
using Artech.Genexus.Common.Parts;
using Genexus.Design.Grammars;
using GeneXus.Design.Grammars.DesignStyles;
using GeneXus.Design.Grammars.Utils;
using GeneXus.Editors.WebEditors.Editor;
using GeneXus.Editors.WebEditors.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Genexus.Design.Grammars.DesignStylesListenerImpl;

namespace GeneXus.Editors.DesignSystem
{
	public abstract class BoundHost<T> : IDocumentHost where T : KBObject
	{
		protected AbstractTextEditor m_editor;
		protected T m_documentObject;

		public BoundHost(AbstractTextEditor editor, T documentObject)
		{
			m_editor = editor;
			m_documentObject = documentObject as T;
		}

		public void SetDocumentObject(KBObject documentObject)
		{
			Debug.Assert(documentObject is T, "Document object must be of " + typeof(T).Name);
			m_documentObject = (documentObject as T);
		}

		public void OnDocumentChanged()
		{
			m_editor.OnDocumentChanged();
		}

		public abstract string Render(string text);
		public abstract List<string> GetTokens(string group);
		public abstract List<string> GetClasses();
		public abstract Dictionary<string, List<ReferenceInfo>> GetClassesReferences();
		public abstract void GoToReference(string type, string name);
		public abstract List<string> GetImages();
		public abstract List<string> GetFiles();
		public abstract List<string> GetAllDSOs();
		public abstract List<GxProperty> GetGxProperties();
		public abstract Dictionary<string,string> GetConfigs();
		public abstract void SetConfig(string name, bool value);
		public abstract void SetPosition(IPosition position);

		internal DesignStylesPart GetStylesPart()
		{
			Artech.Genexus.Common.Objects.DesignSystem ds = m_documentObject as Artech.Genexus.Common.Objects.DesignSystem;
			if (ds != null)
				return ds.Parts.Get<DesignStylesPart>();
			return null;
		}
		internal DesignTokensPart GetTokensPart()
		{
			Artech.Genexus.Common.Objects.DesignSystem ds = m_documentObject as Artech.Genexus.Common.Objects.DesignSystem;
			if (ds != null)
				return ds.Parts.Get<DesignTokensPart>();
			return null;
		}

	}

	public class DesignSystemPartHost<T> : BoundHost<Artech.Genexus.Common.Objects.DesignSystem>, IGXObjectForScripting where T : KBObjectPart
	{
		private DesignSystemHelper m_Helper;
		private DesignSystemHelper Helper
		{
			get {
				if (m_Helper == null)
					m_Helper = new DesignSystemHelper(m_documentObject);
				return m_Helper;
			}
		}
		public DesignSystemPartHost(AbstractTextEditor editor, Artech.Genexus.Common.Objects.DesignSystem obj)
			: base(editor, obj)
		{
			DesignStylesParserValidator.StylesDataNotValid += DesignStylesPart_StylesDataNotValid;
			DesignStylesParserValidator.StylesDataValid += DesignStylesPart_StylesDataValid;
		}

		private void DesignStylesPart_StylesDataValid(object sender, EditorErrorInfo e)
		{
			if (!m_editor.Browser.IsDisposed)
				m_editor.ExecuteJavaScriptAsync("CleanMarkers()");
		}

		private void DesignStylesPart_StylesDataNotValid(object sender, EditorErrorInfo e)
		{
			if (!m_editor.Browser.IsDisposed)
			{
				string[] parms = new string[1];
				parms[0] = JsonConvert.SerializeObject(e.Errors);
				m_editor.Browser.ExecuteMethodAsync("SetErrorMarkers", parms);
			}
		}

		public override string Render(string text)
		{
			return "Not implemented yet!!";
		}

		public override void GoToReference(string type, string name)
		{
			QualifiedName qname = new QualifiedName(name);
			KBObject kbObject = KBObject.Get(m_documentObject.Model, Guid.Parse(type), qname);
			UIServices.DocumentManager.OpenDocument(kbObject, OpenDocumentOptions.CurrentVersion);
		}

		public void Initialize(AbstractWebEditor editor)
		{
		}

		public override List<GxProperty> GetGxProperties()
		{
			DesignStylesPart part = GetStylesPart();
			if (part != null)
				return part.GetGxProperties();
			
			return null;
		}
		public override void SetPosition(IPosition position)
		{
			PositionRange positionRange = null;
			if (position is ClassPosition)
			{
				string className = ((ClassPosition)position).ClassName;
				positionRange = Helper.GetClassPosition(className);
			} else if (position is TextPosition)
			{
				TextPosition textPosition = (TextPosition)position;
				positionRange = new PositionRange(textPosition.Line, textPosition.Line, textPosition.Char, textPosition.Char + textPosition.SelectionLength, 0, 0);
			}
			CallSetPosition(positionRange);
		}

		private void CallSetPosition(PositionRange positionRange)
		{
			string[] parms = new string[1];
			parms[0] = JsonConvert.SerializeObject(positionRange);
			if (m_editor.Browser == null)
				m_editor.InitializeEditor();
			m_editor.Browser.ExecuteMethodAsync("SetPosition", parms);
		}
		public override Dictionary<string, string> GetConfigs() 
			=> Helper.GetUserConfig();

		public override void SetConfig(string name, bool value)
			=> Helper.SetUserConfig(name, value);
		
		//Helper delegation:
		public override List<string> GetTokens(string group)
			=> Helper.GetTokens(group);

		public override List<string> GetClasses()
			=> Helper.GetClasses();
		
		public override Dictionary<string, List<ReferenceInfo>> GetClassesReferences()
			=> Helper.GetClassesReferences();

		public override List<string> GetImages()
			=> Helper.GetImages();

		public override List<string> GetFiles()
			=> Helper.GetFiles();

		public override List<string> GetAllDSOs()
			=> Helper.GetAllDSOs();
	}
}