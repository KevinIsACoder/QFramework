namespace KLFramework.Avatar
{
    //******************************************
    // IAvatarRule
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-08-01 10:30
    //******************************************
    public interface IAvatarRule<C>
    {
        int[] GetMuteParts(int partType, IAvatarPart<C> part);
    }
}