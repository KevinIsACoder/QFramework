namespace KLFramework.Avatar
{
    //******************************************
    // IAvatarLifeCycle
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-08-01 10:32
    //******************************************
    public interface IAvatarLifeCycle<C>
    {
        void OnRemovePart(IAvatar<C> avatar, IAvatarPart<C> removedPart);
        void OnAddPart(IAvatar<C> avatar, IAvatarPart<C> addedPart);
        void OnChangeSkeleton(IAvatar<C> avatar, ISkeleton oldSkeleton, ISkeleton skeleton);
    }
}