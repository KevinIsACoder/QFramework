using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif

    public class LocalizeTarget_UnityUI_RectTransform : LocalizeTarget<UnityEngine.RectTransform>
    {
        static LocalizeTarget_UnityUI_RectTransform() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<UnityEngine.RectTransform, LocalizeTarget_UnityUI_RectTransform>() { Name = "RectTransform", Priority = 100 }); }

        bool isFlip = false;
        bool isCoordinate = false;

        bool mIsRTL;
        bool mInitialize = true;
        float mInitializePosX;
        float mInitializeScaleX;

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.RectTransform; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; }
        public override bool CanUseSecondaryTerm() { return false; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return true; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = null;
            secondaryTerm = null;
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            //--[ Localize Font Object ]----------

            mIsRTL = LocalizationManager.IsRight2Left;

            if (mInitialize)
            {// 初始化
                mInitialize = false;
                mInitializePosX = mTarget.localPosition.x;
                mInitializeScaleX = mTarget.localScale.x;
            }

            if (cmp.CorrectAlignmentForRTL)
            {
                if (mIsRTL)
                {
                    if (isFlip) mTarget.localScale = new Vector3(-mInitializeScaleX, mTarget.localScale.y, mTarget.localScale.z);

                    if (isCoordinate) mTarget.localPosition = new Vector3(-mInitializePosX, mTarget.localPosition.y, mTarget.localPosition.z);
                }
                else
                {
                    if (isFlip) mTarget.localScale = new Vector3(mInitializeScaleX, mTarget.localScale.y, mTarget.localScale.z);

                    if (isCoordinate) mTarget.localPosition = new Vector3(mInitializePosX, mTarget.localPosition.y, mTarget.localPosition.z);
                }


#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(mTarget);
#endif
            }

        }
        // 设置 翻转，改变坐标
        public void SetFlipAndCoordinate(bool flip, bool coordinate)
        {
            isFlip = flip;
            isCoordinate = coordinate;
        }
    }
}
