using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PcapDotNet.Core;
using PcapDotNet.Packets;

using SharpPcap.LibPcap;
using SharpPcap;
using PacketDotNet;

namespace pcapTest
{
    public partial class Interfaces : Form
    {
        List<LibPcapLiveDevice> interfaceList =  new List<LibPcapLiveDevice>();

        public Interfaces()
        {
            InitializeComponent();
        }

        private void Interfaces_Load(object sender, EventArgs e)
        {
            LibPcapLiveDeviceList devices = LibPcapLiveDeviceList.Instance;

            foreach (LibPcapLiveDevice device in devices)
            {
                if (!device.Interface.Addresses.Exists(a => a != null && a.Addr != null && a.Addr.ipAddress != null)) continue;
                var devInterface = device.Interface;
                var friendlyName = devInterface.FriendlyName;
                var description = devInterface.Description;

                interfaceList.Add(device);
                mInterfaceCombo.Items.Add(friendlyName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(mInterfaceCombo.SelectedIndex >=0 && mInterfaceCombo.SelectedIndex < interfaceList.Count)
            {
                MainForm openMainForm = new MainForm(interfaceList, mInterfaceCombo.SelectedIndex);
                this.Hide();
                openMainForm.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mInterfaceCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        
    }
    }

