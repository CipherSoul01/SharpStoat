using Stoat.Core.Interfaces.Decode;
using Stoat.Core.Interfaces.Leases;

namespace Stoat.Core.Image.Leases;

public static class ImageLease
{
    public static IImageLease Owned(IImage image)
        => Create(image, () =>
        {
            if(image is IDisposable disposable)
                disposable.Dispose();
        });
    
    public static IImageLease Create(IImage image, Action release)
        => new ActionImageLease(image, release);
    
    public static IImageLease NonOwning(IImage image)
        => Create(image, static () => { });
    
    private sealed class ActionImageLease : IImageLease {
        private readonly Action _release;
        private IImage? _image;
        private int _disposed;

        public ActionImageLease(IImage image, Action release) {
            _image = image ?? throw new ArgumentNullException(nameof(image));
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        public IImage Image {
            get {
                var image = Volatile.Read(ref _image);
                if (Volatile.Read(ref _disposed) != 0 || image is null)
                    throw new ObjectDisposedException(nameof(ActionImageLease));

                return image;
            }
        }

        public void Dispose() {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            Interlocked.Exchange(ref _image, null);
            _release();
        }
    }
}