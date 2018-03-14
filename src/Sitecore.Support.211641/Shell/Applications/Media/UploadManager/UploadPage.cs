using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.Shell.Applications.Media.UploadManager
{
  public class UploadPage : Sitecore.Shell.Applications.Media.UploadManager.UploadPage
  {
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      if (this.requestLengthExceeded)
      {
        string arg = Translate.Text("The file is too big to be uploaded.\n\nThe maximum size of a file that can be uploaded is {0}.", new object[]
        {
      MainUtil.FormatSize(Settings.Upload.MaximumDatabaseUploadSize)
        }).Replace("\n", string.Empty);
        System.Web.HttpContext.Current.Response.Write(string.Format("<script>alert('{0}');window.parent.close();</script>", arg));
        System.Web.HttpContext.Current.Response.End();
        return;
      }
      ShellPage.IsLoggedIn();
      if (base.IsPostBack)
      {
        ListString listString = new ListString(StringUtil.GetString(new string[]
        {
      base.Request.Form["UploadedItems"]
        }));
        try
        {
          this.UploadFiles(listString);
        }
        catch
        {
          this.ErrorText.Value = Translate.Text("One or more files could not be uploaded.\n\nSee the Log file for more details.");
        }
        string text = ID.NewID.ToShortID().ToString();
        this.UploadedItemsHandle.Value = text;
        WebUtil.SetSessionValue(text, listString.ToString());
        this.UploadedItems.Value = listString.ToString();
      }
      else
      {
        ItemUri itemUri = ItemUri.ParseQueryString(Context.ContentDatabase);
        Assert.IsNotNull(itemUri, typeof(ItemUri));
        this.Uri.Value = itemUri.ToString();
        this.Versioned.Checked = Settings.Media.UploadAsVersionableByDefault;
      }
      this.AsFiles.Visible = !Settings.Media.DisableFileMedia;
      if (Settings.Media.UploadAsFiles)
      {
        this.AsFiles.Checked = true;
      }
      if (Settings.Upload.SimpleUploadOverwriting)
      {
        this.Overwrite.Checked = true;
      }
      if (!Settings.Upload.UserSelectableDestination)
      {
        this.AsFilesCell.Style["display"] = "none";
      }
    }
  }
}