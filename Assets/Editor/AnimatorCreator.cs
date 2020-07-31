using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimatorCreator : MonoBehaviour {

    [SerializeField] BodyPart bodyPart = BodyPart.Head;
    [SerializeField] int numOfPart = 0;
    [SerializeField] bool start = false;

    [SerializeField] Animator HeadAnimator;
    [SerializeField] Animator BodyAnimator;
    [SerializeField] Animator LagsAnimator;

    Sprite[] spritesLagsGoSide;
    Sprite[] spritesBodyGoSide;
    Sprite[] spritesBodyGoSideWith;

    Sprite[] spritesLagsGoBack;
    Sprite[] spritesBodyGoBack;

    Sprite[] spritesLagsGoFront;
    Sprite[] spritesBodyGoFront;

    Sprite[] spritesHead;

    public bool initializated = false;

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
        return new Sprite[1] { startSprites[0] };
    }

    AnimationClip ClipFromSprites(Sprite[] sprites, string name, bool needMove = false)
    {
        //if (AssetDatabase.FindAssets(name + ".anim").Length > 0) return null;
        AnimationClip animClip = new AnimationClip
        {
            frameRate = 25,
            name = name
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

        if (needMove)
        {
            var keys = new Keyframe[3];
            keys[0] = new Keyframe(0.0f, 0.4f);
            keys[1] = new Keyframe(0.15f, 0.35f);
            keys[2] = new Keyframe(0.3f, 0.4f);
            var curve = new AnimationCurve(keys);
            animClip.SetCurve("", typeof(Transform), "localPosition.y", curve);

            keys = new Keyframe[1];
            keys[0] = new Keyframe(0.0f, -0.0011f);
            curve = new AnimationCurve(keys);
            animClip.SetCurve("", typeof(Transform), "localPosition.z", curve);
        }
        AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);

        AssetDatabase.CreateAsset(animClip, "Assets/Editor/" + name + ".anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return animClip;
    }

    void Update()
    {
        if (start)
        {
            start = false;
            ChangeSprites(numOfPart, numOfPart, numOfPart);
        }
    }

    public void ChangeSprites(int x, int y, int z)
    {
        initializated = true;

        switch (bodyPart) {
            case BodyPart.Legs:
                spritesLagsGoSide = LongSprite(Resources.LoadAll<Sprite>("SpritesForBody/Lags" + x + "Side"));
                spritesLagsGoBack = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Lags" + x + "Back"));
                spritesLagsGoFront = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Lags" + x + "Front"));
                var nameOfAnimator = "Lags" + x + "Animator";
                //if (AssetDatabase.FindAssets(name + ".controller").Length > 0) return;
                AnimatorOverrideController aoc = new AnimatorOverrideController(LagsAnimator.runtimeAnimatorController);
                    var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                    foreach (var a in aoc.animationClips)
                    {
                        switch (a.name)
                        {
                            case "Legs1GoSide":
                                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesLagsGoSide, "Legs" + x + "GoSide")));
                                break;
                            case "Legs1StaySide":
                                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesLagsGoSide), "Legs" + x + "StaySide")));
                                break;
                            case "Legs1GoBack":
                                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesLagsGoBack, "Legs" + x + "GoBack")));
                                break;
                            case "Legs1StayBack":
                                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesLagsGoBack), "Legs" + x + "StayBack")));
                                break;
                            case "Legs1GoForvard":
                                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesLagsGoFront, "Legs" + x + "GoForvard")));
                                break;
                            case "Legs1Stay":
                                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesLagsGoFront), "Legs" + x + "Stay")));
                                break;
                        }
                    }
                    aoc.ApplyOverrides(anims);
                    LagsAnimator.runtimeAnimatorController = aoc;
                AssetDatabase.CreateAsset(aoc, "Assets/Editor/" + nameOfAnimator + ".controller");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();


                break;
            case BodyPart.Body:
                spritesBodyGoSide = LongSprite(Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Side"));
                spritesBodyGoSideWith = SuperShortSprite(new Sprite[1] { Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Side")[3] });
                spritesBodyGoBack = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Back"));
                spritesBodyGoFront = ShortSprite(Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Front"));
                nameOfAnimator = "Body" + x + "Animator";
                //if (AssetDatabase.FindAssets(name + ".controller").Length > 0) return;
                AnimatorOverrideController aoc2 = new AnimatorOverrideController(BodyAnimator.runtimeAnimatorController);
                    var anims2 = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                    foreach (var a in aoc2.animationClips)
                    {
                        switch (a.name)
                        {
                            case "Body2GoSide":
                                anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesBodyGoSide, "Body" + y + "GoSide")));
                                break;
                            case "Body2GoSideWith":
                                anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesBodyGoSideWith, "Body" + y + "GoSideWith")));
                                break;
                            case "Body2StaySide":
                                anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesBodyGoSide), "Body" + y + "StaySide")));
                                break;
                            case "Body2GoBack":
                                anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesBodyGoFront, "Body" + y + "GoBack")));
                                break;
                            case "Body2StayBack":
                                anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesBodyGoFront), "Body" + y + "StayBack")));
                                break;
                            case "Body2GoForvard":
                                anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(spritesBodyGoBack, "Body" + y + "GoForvard")));
                                break;
                            case "Body2Stay":
                                anims2.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(SuperShortSprite(spritesBodyGoBack), "Body" + y + "Stay")));
                                break;
                        }
                    }
                    aoc2.ApplyOverrides(anims2);
                    BodyAnimator.runtimeAnimatorController = aoc2;
                AssetDatabase.CreateAsset(aoc2, "Assets/Editor/" + nameOfAnimator + ".controller");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                break;

            case BodyPart.Head:
                spritesHead = Resources.LoadAll<Sprite>("SpritesForBody/Head" + z);
                nameOfAnimator = "Head" + x + "Animator";
                //if (AssetDatabase.FindAssets(name + ".controller").Length > 0) return;
                AnimatorOverrideController aoc3 = new AnimatorOverrideController(HeadAnimator.runtimeAnimatorController);
                var anims3 = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                foreach (var a in aoc3.animationClips)
                {
                    switch (a.name)
                    {
                        case "Head1GoSide":
                            anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[2] }, "Head" + z + "GoSide", true)));
                            break;
                        case "Head1StaySide":
                            anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[2] }, "Head" + z + "StaySide")));
                            break;
                        case "Head1GoBack":
                            anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[1] }, "Head" + z + "GoBack", true)));
                            break;
                        case "Head1StayBack":
                            anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[1] }, "Head" + z + "StayBack")));
                            break;
                        case "Head1GoForvard":
                            anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[0] }, "Head" + z + "GoForvard", true)));
                            break;
                        case "Head1Stay":
                            anims3.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, ClipFromSprites(new Sprite[1] { spritesHead[0] }, "Head" + z + "Stay")));
                            break;
                    }
                }
                aoc3.ApplyOverrides(anims3);
                HeadAnimator.runtimeAnimatorController = aoc3;
                AssetDatabase.CreateAsset(aoc3, "Assets/Editor/" + nameOfAnimator + ".controller");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                break;
    }
    }

    enum BodyPart
    {
        Head,
        Body,
        Legs
    }
}
