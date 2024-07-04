using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace WechatCleanerPlus
{
    public partial class LoadingForm : Form
    {
        private CancellationTokenSource cancellationTokenSource;

        public LoadingForm()
        {
            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        private void LoadingForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

    }
}
