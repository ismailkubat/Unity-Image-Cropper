using UnityEngine;
using UnityEngine.UI;

//Ismail Kubat.
public class ImageCropper : MonoBehaviour 
{
    private Slider slider;
    public Image sourceImage;
    public RectTransform cropRectArea;
    public RawImage croppedImage;
    private Vector2 offset;
    private Vector2 defaultSizeDelta;
    private Vector2 maxSizeDelta;
    private bool onPointerDown;
    private Texture2D croppedTexture;
    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();
    }
    public void UploadTexture(Texture2D texture)
    {
        sourceImage.sprite = GetSpriteFromTexture2D(texture);
    }
    private static Sprite GetSpriteFromTexture2D(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2);
    }
    public Texture GetCroppedTexture()
    {
        return croppedTexture;
    }
    private void Update()
    {
        if (Input.touchCount > 1)
            return;
        if (onPointerDown)
        {
            if (Input.GetMouseButtonDown(0))
            {
                offset = sourceImage.rectTransform.anchoredPosition - (Vector2)Input.mousePosition;
                return;
            }
            if (Input.GetMouseButton(0))
            {
                sourceImage.rectTransform.anchoredPosition = GetClampAnchorPosition(true);
                offset = sourceImage.rectTransform.anchoredPosition - (Vector2)Input.mousePosition;
            }
        }
    }
    private Vector2 GetClampAnchorPosition(bool includeInput)
    {
        Vector2 minRectPosition = cropRectArea.anchoredPosition + cropRectArea.rect.min;
        Vector2 maxRectPosition = cropRectArea.anchoredPosition + cropRectArea.rect.max;
        Vector2 sizeDelta = sourceImage.rectTransform.sizeDelta / 2;
        Vector2 maxAnchoredPosition = minRectPosition + sizeDelta;
        Vector2 minAnchoredPosition = maxRectPosition - sizeDelta;
        Vector2 relativeAnchorPosition = includeInput ? (Vector2)Input.mousePosition + offset : sourceImage.rectTransform.anchoredPosition;
        relativeAnchorPosition.x = Mathf.Clamp(relativeAnchorPosition.x, minAnchoredPosition.x, maxAnchoredPosition.x);
        relativeAnchorPosition.y = Mathf.Clamp(relativeAnchorPosition.y, minAnchoredPosition.y, maxAnchoredPosition.y);
        return relativeAnchorPosition;
    }
    private Texture2D ResizeTexture2D(Texture2D originalTexture, int resizedWidth, int resizedHeight)
    {
        RenderTexture renderTexture = new RenderTexture(resizedWidth, resizedHeight, 32);
        RenderTexture.active = renderTexture;
        Graphics.Blit(originalTexture, renderTexture);
        Texture2D resizedTexture = new Texture2D(resizedWidth, resizedHeight);
        resizedTexture.ReadPixels(new Rect(0, 0, resizedWidth, resizedHeight), 0, 0);
        resizedTexture.Apply();
        return resizedTexture;
    }

    public void Crop()
    {
        if (sourceImage.sprite == null)
        {
            Debug.Log("Empty source image for 'Source' ! Please assign a source image.");
            return;
        }
        Texture2D croppedTexture = new Texture2D((int)cropRectArea.rect.width, (int)cropRectArea.rect.height);
        Texture2D originalTexture = (Texture2D)sourceImage.mainTexture;
        Texture2D originalTextureResized = ResizeTexture2D(originalTexture, (int)sourceImage.rectTransform.rect.width, (int)sourceImage.rectTransform.rect.height);
        float minY = cropRectArea.position.y + cropRectArea.rect.yMin;
        float minX = cropRectArea.position.x + cropRectArea.rect.xMin;
        Vector2 cropPoint = sourceImage.rectTransform.InverseTransformPoint(new Vector2(minX, minY));
        cropPoint.x += sourceImage.rectTransform.rect.width / 2;
        cropPoint.y += sourceImage.rectTransform.rect.height / 2;
        croppedTexture.SetPixels(originalTextureResized.GetPixels((int)cropPoint.x, (int)cropPoint.y, (int)cropRectArea.rect.width, (int)cropRectArea.rect.height));
        croppedTexture.Apply();
        croppedImage.texture = croppedTexture;
    }

    private void ConvertImageSize()
    {
        float width = sourceImage.mainTexture.width;
        float height = sourceImage.mainTexture.height;
        Vector2 sizeDelta = Vector2.zero;

        if (height > width)
        {
            float rate = height / width;
            sizeDelta.x = cropRectArea.rect.width;
            sizeDelta.y = sizeDelta.x * rate;

        }
        else if (width > height)
        {
            float rate = width / height;
            sizeDelta.y = cropRectArea.rect.height;
            sizeDelta.x = sizeDelta.y * rate;
        }
        else
            sizeDelta = cropRectArea.sizeDelta;

        defaultSizeDelta = sizeDelta;
        maxSizeDelta = sizeDelta * 3;
        sourceImage.rectTransform.sizeDelta = sizeDelta;
        sourceImage.rectTransform.anchoredPosition = GetClampAnchorPosition(false);
    }
    public void OnChangeSliderValue()
    {
        float value = slider.value;
        float newSizeDeltaX = UtilScript.Remap(value, 0f, 1f, defaultSizeDelta.x, maxSizeDelta.x);
        float newSizeDeltaY = UtilScript.Remap(value, 0f, 1f, defaultSizeDelta.y, maxSizeDelta.y);
        sourceImage.rectTransform.sizeDelta = new Vector2(newSizeDeltaX, newSizeDeltaY);
        sourceImage.rectTransform.anchoredPosition = GetClampAnchorPosition(false);
    }
    public void OnPointerDownCropperArea()
    {
        onPointerDown = true;
    }
    public void OnPointerUpCropperArea()
    {
        onPointerDown = false;
    }
    public void PickImage()
    {
        //To be able to upload a photo from the gallery, you need to import the "NativeGallery.package"! .
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize: -1);
                if (texture == null)
                {
                    Debug.Log("Fotoðraf yüklenirken bir hata oluþtu!");
                    return;
                }
                sourceImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                sourceImage.rectTransform.anchoredPosition = Vector2.zero;
                ConvertImageSize();
                slider.value = 0;
                OnChangeSliderValue();
            }
        }, "Galeriden fotoðraf seç");
        Debug.Log("Ýzin durumu: " + permission);
    }
}
