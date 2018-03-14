using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Upload;
using Sitecore.Shell.Web;
using Sitecore.Text;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Sitecore.Support.Shell.Applications.Media.UploadManager
{
  public class UploadPage : Sitecore.Shell.Applications.Media.UploadManager.UploadPage
  {
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      if ((bool)typeof(Sitecore.Shell.Applications.Media.UploadManager.UploadPage)
        .GetField("requestLengthExceeded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this))
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
        string text = Sitecore.Data.ID.NewID.ToShortID().ToString();
        this.UploadedItemsHandle.Value = text;
        WebUtil.SetSessionValue(text, listString.ToString());
        this.UploadedItems.Value = listString.ToString();
      }
      else
      {
        ItemUri itemUri = ItemUri.ParseQueryString(Sitecore.Context.ContentDatabase);
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

    private void UploadFiles(ListString items)
    {
      Assert.ArgumentNotNull(items, "items");
      if (base.Request.Files.Count > 0)
      {
        string @string = StringUtil.GetString(new string[]
        {
      base.Request.Form["Uri"]
        });
        bool overwrite = StringUtil.GetString(new string[]
        {
      base.Request.Form["Overwrite"]
        }) == "1";
        bool unpack = StringUtil.GetString(new string[]
        {
      base.Request.Form["Unpack"]
        }) == "1";
        bool versioned = StringUtil.GetString(new string[]
        {
      base.Request.Form["Versioned"]
        }) == "1";
        bool flag = StringUtil.GetString(new string[]
        {
      base.Request.Form["AsFiles"]
        }) == "1";
        string folder = string.Empty;
        Language language = Sitecore.Context.ContentLanguage;
        ItemUri itemUri = ItemUri.Parse(@string);
        if (itemUri != null)
        {
          folder = itemUri.GetPathOrId();
          language = itemUri.Language;
        }
        UploadArgs uploadArgs = new UploadArgs();
        uploadArgs.Files = base.Request.Files;
        uploadArgs.Folder = folder;
        uploadArgs.Overwrite = overwrite;
        uploadArgs.Unpack = unpack;
        uploadArgs.Versioned = versioned;
        uploadArgs.Language = language;
        uploadArgs.CloseDialogOnEnd = false;
        uploadArgs.Destination = (flag ? UploadDestination.File : UploadDestination.Database);
        Pipeline pipeline = PipelineFactory.GetPipeline("uiUpload");
        try
        {
          pipeline.Start(uploadArgs);
        }
        catch (Exception ex)
        {
          if (ex.InnerException is OutOfMemoryException)
          {
            uploadArgs.ErrorText = Translate.Text("A file is too big to be uploaded.\n\nThe maximum size of a file that can be uploaded is {0}.", new object[]
            {
          MainUtil.FormatSize(Settings.Media.MaxSizeInDatabase)
            });
          }
          else
          {
            uploadArgs.ErrorText = "An error occured while uploading:\n\n" + ex.InnerException.Message;
          }
        }
        if (!string.IsNullOrEmpty(uploadArgs.ErrorText))
        {
          this.ErrorText.Value = uploadArgs.ErrorText;
        }
        foreach (Item current in uploadArgs.UploadedItems)
        {
          string text = current.Uri.ToString();
          if (!items.Contains(text))
          {
            items.Add(text);
          }
        }
      }
    }
  }
}