using System;
using System.Drawing;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class ImageRef : Present, IDisposable
    {
        private ImageRefCounted resource;
        private string refCredit;

        public GDIBigLockedImage image
        {
            get
            {
                return resource.image;
            }
        }

        public ImageRef(ImageRefCounted resource) : this(resource, "new")
        {
        }

        public ImageRef(ImageRefCounted resource, string refCredit)
        {
            resource.refCreditCounter++;
            this.refCredit = string.Format("{0}-{1}", refCredit, resource.refCreditCounter);
            resource.AddRef(this.refCredit);
            this.resource = resource;
        }

        public void Dispose()
        {
            resource.DropRef(refCredit);
        }

        public Present Duplicate(string refCredit)
        {
            return new ImageRef(resource, refCredit);
        }

        public ImageRef Copy()
        {
            GDIBigLockedImage image;
            Monitor.Enter(image = this.image);
            ImageRef result;
            try
            {
                Image original = this.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                result = new ImageRef(new ImageRefCounted(new GDIBigLockedImage(new Bitmap(original))));
            }
            finally
            {
                Monitor.Exit(image);
            }

            return result;
        }
    }
}
