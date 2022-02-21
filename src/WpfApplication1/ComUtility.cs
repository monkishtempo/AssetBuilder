using System.Runtime.InteropServices;

namespace AssetBuilder
{
    [ComVisible(true)]
    public interface IComInterface
    {
        void openNode(int id, int AssetTypeID);
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    class ComClass : IComInterface
    {
        #region IComInterface Members

        public void openNode(int id, int AssetTypeID)
        {
            System.Windows.Forms.MessageBox.Show("Test");
        }

        #endregion
    }
}
