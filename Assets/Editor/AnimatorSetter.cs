using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimatorSetter : MonoBehaviour {

    [SerializeField] Animator HeadAnimator;
    [SerializeField] Animator BodyAnimator;
    [SerializeField] Animator LagsAnimator;

    Sprite[] spritesLagsGoSide;
    Sprite[] spritesBodyGoSide;

    Sprite[] spritesLagsGoBack;
    Sprite[] spritesBodyGoBack;

    Sprite[] spritesLagsGoFront;
    Sprite[] spritesBodyGoFront;

    Sprite[] spritesHead;

    public bool initializated = false;

    // Use this for initialization
    void Start () {
		
	}

    Sprite[] LongSprite(Sprite[] startSprites)
    {
        return new Sprite[6] { startSprites[1], startSprites[3], startSprites[2], startSprites[1], startSprites[0], startSprites[0] };
    }

    Sprite[] ShortSprite(Sprite[] startSprites)
    {
        return new Sprite[4] { startSprites[1], startSprites[2], startSprites[1], startSprites[0] };
    }

    Sprite[] SuperShortSprite(Sprite[] startSprites)
    {
        return new Sprite[1] { startSprites[0]};
    }

    AnimationClip ClipFromSprites(Sprite[] sprites, bool needMove = false)
    {
        AnimationClip animClip = new AnimationClip
        {
            frameRate = 25,
            name = "123"
        };

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < (sprites.Length); i++)
        {
            spriteKeyFrames[i] = new ObjectReferenceKeyframe
            {
                time = i * 0.1f,
                value = sprites[i]
            };
        }

        if(needMove)
        {
            var keys = new Keyframe[3];
            keys[0] = new Keyframe(0.0f, 0.4f);
            keys[1] = new Keyframe(0.15f, 0.35f);
            keys[2] = new Keyframe(0.3f, 0.4f);
            var curve = new AnimationCurve(keys);
            animClip.SetCurve("", typeof(Transform), "localPosition.y", curve);

            keys = new Keyframe[1];
            keys[0] = new Keyframe(0.0f, -0.002f);
            curve = new AnimationCurve(keys);
            animClip.SetCurve("", typeof(Transform), "localPosition.z", curve);
        }
        AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);
        return animClip;
    }

    public void ChangeSprites(int x, int y, int z)
    {
        initializated = true;

        spritesLagsGoSide = LongSprite(Resources.LoadAll<Sprite>("SpritesForBody/Lags" + x + "Side"));
        spritesLagsGoBack = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Lags" + x + "Back"));
        spritesLagsGoFront = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Lags" + x + "Front"));
        spritesBodyGoSide = LongSprite(Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Side"));
        spritesBodyGoBack = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Back"));
        spritesBodyGoFront = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Front"));
        spritesHead = Resources.LoadAll<Sprite>("SpritesForBody/Head" + z);

        AnimatorOverrideController aoc = new AnimatorOverrideController(LagsAnimator.runtimeAnimatorController);
        var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var a in aoc.animationClips)
        {
            switch (a.name)
            {
                case "Legs1GoSide":
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesLagsGoSide)));
                    break;
                case "Legs1StaySide":
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesLagsGoSide))));
                    break;
                case "Legs1GoBack":
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesLagsGoBack)));
                    break;
                case "Legs1StayBack":
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesLagsGoBack))));
                    break;
                case "Legs1GoForvard":
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesLagsGoFront)));
                    break;
                case "Legs1Stay":
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesLagsGoFront))));
                    break;
            }
        }
        aoc.ApplyOverrides(anims);
        LagsAnimator.runtimeAnimatorController = aoc;


        AnimatorOverrideController aoc2 = new AnimatorOverrideController(BodyAnimator.runtimeAnimatorController);
        var anims2 = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var a in aoc2.animationClips)
        {
            switch (a.name)
            {
                case "Body2GoSide":
                    anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesBodyGoSide)));
                    break;
                case "Body2StaySide":
                    anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesBodyGoSide))));
                    break;
                case "Body2GoBack":
                    anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesBodyGoFront)));
                    break;
                case "Body2StayBack":
                    anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesBodyGoFront))));
                    break;
                case "Body2GoForvard":
                    anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesBodyGoBack)));
                    break;
                case "Body2Stay":
                    anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesBodyGoBack))));
                    break;
            }
        }
        aoc2.ApplyOverrides(anims2);
        BodyAnimator.runtimeAnimatorController = aoc2;

        AnimatorOverrideController aoc3 = new AnimatorOverrideController(HeadAnimator.runtimeAnimatorController);
        var anims3 = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var a in aoc3.animationClips)
        {
            switch (a.name)
            {
                case "Head1GoSide":
                    anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[2] }, true)));
                    break;
                case "Head1StaySide":
                    anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[2] })));
                    break;
                case "Head1GoBack":
                    anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[1] }, true)));
                    break;
                case "Head1StayBack":
                    anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[1] })));
                    break;
                case "Head1GoForvard":
                    anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[0] }, true)));
                    break;
                case "Head1Stay":
                    anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[0] })));
                    break;
            }
        }
        aoc3.ApplyOverrides(anims3);
        HeadAnimator.runtimeAnimatorController = aoc3;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
