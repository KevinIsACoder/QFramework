namespace KLFramework.Avatar
{

    //******************************************
    // DelegateLifeCycle
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-08-01 18:15
    //******************************************
    public class DelegateLifeCycle<C> : IAvatarLifeCycle<C>
    {
        public delegate void OnRemovePartHandler(IAvatar<C> avatar, IAvatarPart<C> removedPart);
        public delegate void OnAddPartHandler(IAvatar<C> avatar, IAvatarPart<C> addedPart);
        public delegate void OnChangeSkeletonHandler(IAvatar<C> avatar, ISkeleton oldSkeleton, ISkeleton skeleton);

        private event OnRemovePartHandler _OnRemovePartHandler = ((avatar, part) => { });
        private event OnAddPartHandler _OnAddPartHandler = ((avatar, part) => { });
        private event OnChangeSkeletonHandler _OnChangeSkeletonHandler = ((avatar, oldSkeleton, skeleton) => { });
        
        public DelegateLifeCycle<C> AddOnRemovePartHandler(OnRemovePartHandler onRemovePartHandler)
        {
            _OnRemovePartHandler += onRemovePartHandler;
            return this;
        }
        public DelegateLifeCycle<C> RemoveOnRemovePartHandler(OnRemovePartHandler onRemovePartHandler)
        {
            _OnRemovePartHandler -= onRemovePartHandler;
            return this;
        }
        public DelegateLifeCycle<C> AddOnAddPartHandler(OnAddPartHandler onAddPartHandler)
        {
            _OnAddPartHandler += onAddPartHandler;
            return this;
        }
        public DelegateLifeCycle<C> RemoveOnAddPartHandler(OnAddPartHandler onAddPartHandler)
        {
            _OnAddPartHandler -= onAddPartHandler;
            return this;
        }
        public DelegateLifeCycle<C> AddOnChangeSkeletonHandler(OnChangeSkeletonHandler onChangeSkeletonHandler)
        {
            _OnChangeSkeletonHandler += onChangeSkeletonHandler;
            return this;
        }
        public DelegateLifeCycle<C> RemoveOnChangeSkeletonHandler(OnChangeSkeletonHandler onChangeSkeletonHandler)
        {
            _OnChangeSkeletonHandler -= onChangeSkeletonHandler;
            return this;
        }

        public void OnRemovePart(IAvatar<C> avatar, IAvatarPart<C> removedPart)
        {
            _OnRemovePartHandler(avatar, removedPart);
        }

        public void OnAddPart(IAvatar<C> avatar, IAvatarPart<C> addedPart)
        {
            _OnAddPartHandler(avatar, addedPart);
        }

        public void OnChangeSkeleton(IAvatar<C> avatar, ISkeleton oldSkeleton, ISkeleton skeleton)
        {
            _OnChangeSkeletonHandler(avatar, oldSkeleton, skeleton);
        }
    }
}