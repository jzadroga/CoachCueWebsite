using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.IO;
using System.Web.Helpers;

namespace CoachCue.Helpers
{
    public class ThumbnailImageResult : ActionResult
    {
        public string ImageFileName { get; private set; }
        public int Size { get; private set; }

        public ThumbnailImageResult(string imageFileName, int size)
        {
            this.ImageFileName = imageFileName;
            this.Size = size;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (File.Exists(this.ImageFileName))
            {
                Image photoImg = Image.FromFile(this.ImageFileName);

                //get the correct height based on a fixed width
                int width = this.Size;
                int X = photoImg.Width;
                int Y = photoImg.Height;
                int height = (int)((width * Y) / X);

                // Resizing the image on the fly
                WebImage thumb = new WebImage(this.ImageFileName).Resize(width, height, false, true);

                //have to make the picture 48 pixels wide than decide how to crop the height if it is too long
                //otherwise fill in the height?
                if (height > width)
                {
                    var topBottomCrop = (height - width) / 2;
                    thumb = thumb.Crop(topBottomCrop, 0, topBottomCrop, 0);
                }

                var response = context.RequestContext.HttpContext.Response;
                response.ContentType = "image/jpeg";

                thumb.Write();
            }
        }
    }
}