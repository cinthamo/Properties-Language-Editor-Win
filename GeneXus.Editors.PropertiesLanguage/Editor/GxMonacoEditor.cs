using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Genexus.Common.Parts;
using GeneXus.Editors.WebEditors.Editor;
using GeneXus.Editors.WebEditors.Helpers;

namespace GeneXus.Editors.DesignSystem
{
	public partial class GxMonacoEditor
		: AbstractTextEditor, IGxView
	{
		public GxMonacoEditor(string language)
			: base(language)
		{
			//Document.DocumentChanged += Document_DocumentChanged;
		}

		private void Document_DocumentChanged(object sender, DocumentEventArgs e)
		{
			Artech.Genexus.Common.Objects.DesignSystem ds = Document as Artech.Genexus.Common.Objects.DesignSystem;
			if (ds != null)
			{
				DesignStylesPart stylesPart = ds.Parts.Get<DesignStylesPart>();
				stylesPart.Validate();
			}
				 
		}

		public override IGXObjectForScripting GetObjectForScripting()
		{
			return new DesignSystemPartHost<KBObjectPart>(this, null);
		}
	}
}
