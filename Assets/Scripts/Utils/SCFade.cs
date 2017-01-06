using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SCFade : MonoBehaviour
{
	private SpriteRenderer mSprite;
	private MeshRenderer mMesh;
	private GUITexture mTexture;
	private GUIText mText;
	private TextMesh mTextMesh;
	private Text mTextUI;
	private Image mImageUI;
	private CanvasGroup mCanvas;
	private ParticleSystemRenderer mParticle;
	private float fadeValue = 1f;
	private float currentTime = 0f;
	private float timeItTakesToFadeOut = 999999f;
	private float timeItTakesToFadeIn  = 999999f;
	private bool isFading    = false;
	private bool isAppearing = false;
	public bool StartFadedOut = true;
	public bool StartFading = false;
	public float fadingDuration = 0.3f;
	public float fadeInDelay = 0;
	public float fadeOutDelay = 0;

	private float completeFading;

	private Type mType;
	private enum Type
	{
		Uni2DSprite,
		SpriteRenderer,
		MeshRenderer,
		GUITexture,
		GUIText,
		EasyFont,
		TextMesh,
		Text,
		Image,
		Canvas,
		Particle
	}

	private bool mFaded = false;

	public bool disableElements = true;
 
	void Awake() 
	{
		Init();

		if(mType == Type.Particle || mType == Type.MeshRenderer)
				completeFading = fadeValue;

		if (StartFadedOut || StartFading) 
		{
			fadeValue = 0.0f;
			mFaded = true;
			currentTime = 0;
			SetFadeValue();
		}

		if (StartFadedOut && disableElements) 
		{
			EnableRendererAndCollider (false);
		}

		if (StartFading && disableElements) 
		{
			EnableRendererAndCollider (true);
		}
	}

	void Start()
	{
		if(StartFading)
		{
			StartFadeIn(fadingDuration);
		}
	}

	public void Init()
	{
		mCanvas = this.gameObject.GetComponent<CanvasGroup>();		
		mSprite = gameObject.GetComponent<SpriteRenderer>();
		mMesh = this.gameObject.GetComponent<MeshRenderer>();
		mTexture = this.gameObject.GetComponent<GUITexture>();
		mText = this.gameObject.GetComponent<GUIText>();
		mTextMesh = this.gameObject.GetComponent<TextMesh>();
		mTextUI = this.gameObject.GetComponent<Text>();
		mImageUI = this.gameObject.GetComponent<Image>();
		mParticle = this.gameObject.GetComponent<ParticleSystemRenderer>();
		
		if (mTexture == null && mMesh == null && mText == null && mTextMesh == null && mTextUI == null && mImageUI == null && mCanvas == null && mSprite == null) 
			throw new UnityException("Incorrect usage of SCFade. Apply it to Uni2DSprite, GUITexture, GUIText, EasyFontTextMesh, Text, Image, Canvas objects! Object name is " + gameObject.name);

		else if (mCanvas)
		{
			mType = Type.Canvas;
			fadeValue = mCanvas.alpha;
		}

		else if(mTexture)
		{
			mType = Type.GUITexture;
			fadeValue = mTexture.color.a * 2;
		}

		else if(mSprite)
		{
			mType = Type.SpriteRenderer;
			fadeValue = mSprite.color.a;
		}

		
		else if(mText)
		{
			mType = Type.GUIText;
			fadeValue = mText.color.a;
		}
			
		else if(mTextMesh)
		{
			mType = Type.TextMesh;
			fadeValue = mTextMesh.color.a;
		}

		else if(mTextUI)
		{
			mType = Type.Text;
			fadeValue = mTextUI.color.a;
		}

		else if(mImageUI)
		{
			mType = Type.Image;
			fadeValue = mImageUI.color.a;
		}

		else if(mMesh)
		{
			mType = Type.MeshRenderer;
			fadeValue = mMesh.material.GetColor("_TintColor").a;
		}

		else if(mParticle)
		{
			mType = Type.Particle;
			fadeValue = mParticle.material.GetColor("_TintColor").a;
		}
	}
 
	void Update() 
	{
    	if (isFading) 
		{

       		currentTime += Time.deltaTime;
			
        	if (currentTime < timeItTakesToFadeOut) 
          		fadeValue = 1f - (currentTime / timeItTakesToFadeOut);	
        	
			else 
			{
				fadeValue = 0.0f;
           		isFading = false;
				mFaded = true;
				currentTime = 0;

				if (disableElements)
					EnableRendererAndCollider(false);

        	}

			SetFadeValue();
    	}
 
    	else if (isAppearing) 
		{	
       		currentTime += Time.deltaTime;
			
       		if (currentTime < timeItTakesToFadeIn)
         		fadeValue =  (currentTime / timeItTakesToFadeIn);

			else 
			{
				fadeValue = 1.0f;
         		isAppearing = false;
				mFaded = false;
				currentTime = 0;
       		}

			SetFadeValue();
    	}
	}

	public bool isVisible() 
	{
		return !mFaded;
	}
 
	public void StartFadeOut(float duration) 
	{	

		if(mFaded && !isAppearing)
			return;
	
		if(duration != 0)
		{
	    	isFading = true;
			timeItTakesToFadeOut = duration;
			if(isAppearing)
			{
				isAppearing = false;
				currentTime = (1f - fadeValue) * timeItTakesToFadeOut;
			}
		}

		else 
			CompleteFadeOut();
	}
 
	public void StartFadeIn(float duration) 
	{
		if(!mFaded && !isFading)
			return;	

		if (disableElements)
			EnableRendererAndCollider(true);

		if(duration != 0)
		{
	    	isAppearing = true;
			timeItTakesToFadeIn = duration;
			if(isFading)
			{
				isFading = false;
				currentTime = fadeValue * timeItTakesToFadeIn;
			}
		}

		else
			CompleteFadeIn();

	}

	private void CompleteFadeOut()
	{
		fadeValue = 0.0f;
		isFading = false;
		isAppearing = false;
		mFaded = true;
		currentTime = 0;
		
		SetFadeValue();

		if (disableElements)
			EnableRendererAndCollider(false);
	}

	private void CompleteFadeIn()
	{
		fadeValue = 1.0f;
		isFading = false;
		isAppearing = false;
		mFaded = false;
		currentTime = 0;
		
		SetFadeValue();
	}

	private void EnableRendererAndCollider(bool enable) 
	{
		if (gameObject.GetComponent<Collider>() != null) gameObject.GetComponent<Collider>().enabled = enable;
		if (gameObject.GetComponent<Renderer>() != null) gameObject.GetComponent<Renderer>().enabled = enable;
		if (gameObject.GetComponent<CanvasGroup> () != null) 
		{
			gameObject.GetComponent<CanvasGroup> ().blocksRaycasts = enable;
			gameObject.GetComponent<CanvasGroup> ().interactable = enable;
		}
	}

	private void SetFadeValue()
	{
		switch(mType)
		{
			case Type.SpriteRenderer:
				mSprite.color = new Color(mSprite.color.r, mSprite.color.g, mSprite.color.b, fadeValue);
			break;
			
			case Type.GUITexture:
				mTexture.color = new Color(mTexture.color.r, mTexture.color.g, mTexture.color.b , fadeValue/2);
				break;
				
			case Type.GUIText:
				mText.color = mText.color = new Color(mText.color.r, mText.color.g, mText.color.b, fadeValue);
				break;

			case Type.TextMesh:
				mTextMesh.color = new Color(mTextMesh.color.r, mTextMesh.color.g, mTextMesh.color.b, fadeValue);
				break;

			case Type.Text:
				mTextUI.color = new Color(mTextUI.color.r, mTextUI.color.g, mTextUI.color.b, fadeValue);
				break;
			case Type.Image:
				mImageUI.color = new Color(mImageUI.color.r, mImageUI.color.g, mImageUI.color.b, fadeValue);
				break;

			case Type.Canvas:
				mCanvas.alpha = fadeValue;
				break;

			case Type.MeshRenderer:
				Color meshColor = mMesh.material.GetColor("_TintColor");
				FadeMaterial(meshColor);
				mMesh.material.SetColor("_TintColor", meshColor);
				break;
			case Type.Particle:
				Color particleColor = mParticle.material.GetColor("_TintColor");
				FadeMaterial(particleColor);
				mParticle.material.SetColor("_TintColor", particleColor);
				break;
		}

	}

	private void FadeMaterial(Color color)
	{
		color = new Color(color.r, color.g, color.b, fadeValue);
				
		if(color.a == 1f)
			color.a = completeFading;
	}
}
