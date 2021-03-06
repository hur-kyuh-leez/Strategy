﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ShareInvest.Controls;
using ShareInvest.EventHandler;
using ShareInvest.Message;

namespace ShareInvest.Kospi200
{
    public partial class GoblinBat : Form
    {
        public GoblinBat()
        {
            InitializeComponent();
            SuspendLayout();
            ChooseStrategy(TimerBox.Show(new Message().ChooseStrategy, "Option", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, 15325), new Yield(), new SelectStatisticalData());
            Application.DoEvents();
            SetVisibleCore(false);
            ShowDialog();
            Dispose();
            Environment.Exit(0);
        }
        private void ChooseStrategy(DialogResult result, Yield yield, SelectStatisticalData data)
        {
            splitContainerStrategy.Panel1.Controls.Add(yield);
            splitContainerStrategy.Panel2.Controls.Add(data);
            splitContainerBackTesting.Panel1.Controls.Add("");
            yield.Dock = DockStyle.Fill;
            data.Dock = DockStyle.Fill;
            "".Dock = DockStyle.Fill;
            Choice = result;
            Size = new Size(1241, 491);
            splitContainerStrategy.SplitterDistance = 127;
            splitContainerStrategy.Panel1.BackColor = Color.FromArgb(121, 133, 130);
            splitContainerStrategy.Panel2.BackColor = Color.FromArgb(121, 133, 130);
            splitContainerBackTesting.Panel1.BackColor = Color.FromArgb(121, 133, 130);
            yield.SendHermes += data.OnReceiveHermes;
            data.SendStrategy += yield.OnReceiveStrategy;
            data.SendClose += OnReceiveClose;
            Dictionary<string, int> param = new Dictionary<string, int>(1024);

            foreach (string[] temp in yield)
                for (int i = 0; i < temp.Length; i++)
                    param[string.Concat(i, ';', temp[i])] = i;

            data.StartProgress(param);
            SetControlsChangeFont(result, Controls, new Font("Consolas", Font.Size + 0.75F, FontStyle.Regular));
            ResumeLayout();
            Show();
            CenterToScreen();
            Application.DoEvents();
            data.GetStrategy(yield.SetStrategy(TimerBox.Show(new Message().Automatically, "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 21753)));
        }
        private void SetControlsChangeFont(DialogResult result, Control.ControlCollection controls, Font font)
        {
            if (result.Equals(DialogResult.OK))
                foreach (Control control in controls)
                {
                    string name = control.GetType().Name;

                    if (control.Font.Name.Equals("Brush Script Std") && (name.Equals("CheckBox") || name.Equals("Label") || name.Equals("Button") || name.Equals("TabControl")))
                        control.Font = control.Text.Contains(" by Day") ? font : new Font("Consolas", Font.Size + 1.25F, FontStyle.Bold);

                    if (control.Text.Contains("%"))
                        control.Font = new Font("Consolas", Font.Size + 0.75F, FontStyle.Regular);

                    if (control.Controls.Count > 0)
                        SetControlsChangeFont(DialogResult.OK, control.Controls, font);
                }
        }
        private void StartTrading(Balance bal, ConfirmOrder order, ConnectKHOpenAPI api)
        {
            Controls.Add(api);
            splitContainerBalance.Panel1.Controls.Add(order);
            splitContainerBalance.Panel2.Controls.Add(bal);
            api.Dock = DockStyle.Fill;
            order.Dock = DockStyle.Fill;
            bal.Dock = DockStyle.Fill;
            bal.BackColor = Color.FromArgb(203, 212, 206);
            order.BackColor = Color.FromArgb(121, 133, 130);
            api.Hide();
            splitContainerBalance.Panel2MinSize = 3;
            splitContainerBalance.Panel1MinSize = 96;
            order.SendTab += OnReceiveTabControl;
            bal.SendReSize += OnReceiveSize;

            if (Choice.Equals(DialogResult.OK))
                foreach (Control control in order.Controls.Find("checkBox", true))
                    control.Font = new Font("Consolas", control.Font.Size + 0.25F, FontStyle.Bold);

            ResumeLayout();
            Application.DoEvents();
        }
        private void OnReceiveClose(object sender, DialogClose e)
        {
            SuspendLayout();
            BeginInvoke(new Action(() => Strategy = new Strategy(new Specify
            {
                Reaction = e.Reaction,
                ShortDayPeriod = e.ShortDay,
                ShortTickPeriod = e.ShortTick,
                LongDayPeriod = e.LongDay,
                LongTickPeriod = e.LongTick,
                HedgeType = e.Hedge,
                Base = e.Base,
                Sigma = e.Sigma,
                Percent = e.Percent,
                Max = e.Max,
                Quantity = e.Quantity,
                Time = e.Time
            })));
            StartTrading(Balance.Get(), ConfirmOrder.Get(), new ConnectKHOpenAPI());
        }
        private void OnReceiveAccount(object sender, Account e)
        {
            if (e.Server.Equals("1"))
                FormSizes[2, 0] = 594;

            account.Text = e.AccNo;
            id.Text = e.ID;
            Api = ConnectAPI.Get();
            Api.SendDeposit += OnReceiveDeposit;
            Api.LookUpTheDeposit(e.AccNo, true);
        }
        private void OnReceiveDeposit(object sender, Deposit e)
        {
            BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < e.ArrayDeposit.Length; i++)
                    if (e.ArrayDeposit[i].Length > 0)
                        string.Concat("balance", i).FindByName<Label>(this).Text = long.Parse(e.ArrayDeposit[i]).ToString("N0");

                splitContainerAccount.Panel1.BackColor = Color.FromArgb(121, 133, 130);
                splitContainerAccount.Panel2.BackColor = Color.FromArgb(121, 133, 130);
                splitContainerAccount.SplitterWidth = 2;
                long trading = long.Parse(e.ArrayDeposit[20]), deposit = long.Parse(e.ArrayDeposit[18]);

                if (Account == false)
                {
                    bool checkCurrentAsset = deposit < trading && deposit < Deposit ? true : false;
                    Strategy.SetDeposit(new InQuiry
                    {
                        AccNo = account.Text,
                        BasicAssets = checkCurrentAsset ? deposit : Deposit
                    });
                    balance18.ForeColor = Color.GhostWhite;

                    if (checkCurrentAsset)
                    {
                        balance18.ForeColor = Color.DeepSkyBlue;
                        TimerBox.Show("The Current Asset is below the Set Value.\n\nAt least 10% more Assets are Required than 'Back-Testing' for Safe Trading.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, 3715);
                    }
                    return;
                }
                if (Account)
                {
                    string[] assets = new Assets().ReadCSV().Split(',');
                    long temp = 0, backtesting = long.Parse(assets[1]);
                    DialogResult result = TimerBox.Show("Are You using Automatic Login??\n\nThe Automatic Login Compares the Asset setup\namount with the Current Asset during the Back Testing\nand sets a Small amount as a Deposit.\n\nIf You aren't using It,\nClick 'Cancel'.\n\nAfter 10 Seconds,\nIt's Regarded as an Automatic Mode and Proceeds.", "Notice", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 9617);

                    switch (result)
                    {
                        case DialogResult.OK:
                            temp = backtesting >= trading ? trading : backtesting;
                            break;

                        case DialogResult.Cancel:

                            if (TimerBox.Show(string.Concat("The set amount at the Time of the Test is ￦", backtesting.ToString("N0"), "\nand the Current Assets are ￦", trading.ToString("N0"), ".\n\nClick 'Yes' to set it to ￦", backtesting.ToString("N0"), ".\n\nIf You don't Choose,\nYou'll Set it as Current Asset."), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, 9712).Equals(DialogResult.No))
                                temp = trading;

                            else
                                temp = backtesting;

                            break;
                    }
                    Deposit = temp;
                    Account = Strategy.SetAccount(new InQuiry
                    {
                        AccNo = account.Text,
                        BasicAssets = temp
                    });
                }
            }));
        }
        private void OnReceiveSize(object sender, GridReSize e)
        {
            splitContainerBalance.SplitterDistance = splitContainerBalance.Height - e.ReSize - splitContainerBalance.SplitterWidth;
            CenterToScreen();

            if (e.Count < 8)
                FormSizes[2, 1] = 315;

            else if (e.Count > 7)
                FormSizes[2, 1] = 328 + (e.Count - 7) * 21;
        }
        private void OnReceiveTabControl(object sender, Mining e)
        {
            if (e.Tab != 9 && Tap == false)
            {
                Tap = true;
                tabControl.SelectedIndex = e.Tab;
            }
            else if (e.Tab == 9 && Tap)
                Close();
        }
        private void TabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex.Equals(3))
                splitContainerBackTesting.Panel2.BackColor = Color.FromArgb(118, 130, 127);

            Hide();
            SuspendLayout();
            Size = new Size(FormSizes[tabControl.SelectedIndex, 0], FormSizes[tabControl.SelectedIndex, 1]);
            splitContainerBalance.AutoScaleMode = AutoScaleMode.Font;
            CenterToScreen();
            Application.DoEvents();
            ResumeLayout();
            Show();
        }
        private void ServerCheckedChanged(object sender, EventArgs e)
        {
            if (CheckCurrent)
            {
                server.Text = "During Auto Renew";
                server.ForeColor = Color.Ivory;
                account.ForeColor = Color.Ivory;
                id.ForeColor = Color.Ivory;
                timer.Interval = 19531;
                timer.Start();

                return;
            }
            timer.Stop();
            server.Text = "Stop Renewal";
            server.ForeColor = Color.Maroon;
        }
        private void TimerTick(object sender, EventArgs e)
        {
            if (DateTime.Now.Hour > 14 && DateTime.Now.Minute > 34)
            {
                timer.Stop();
                timer.Dispose();
            }
            Api.LookUpTheDeposit(account.Text, Api.OnReceiveBalance);
        }
        private ConnectAPI Api
        {
            get; set;
        }
        private Strategy Strategy
        {
            get; set;
        }
        private DialogResult Choice
        {
            get; set;
        }
        private long Deposit
        {
            get; set;
        }
        private bool CheckCurrent
        {
            get
            {
                return server.Checked;
            }
        }
        private bool Tap
        {
            get; set;
        }
        private bool Account
        {
            get; set;
        } = true;
        private int[,] FormSizes
        {
            get; set;
        } =
        {
            { 1241, 491 },
            { 750, 370 },
            { 602, 315 },
            { 405, 450 }
        };
    }
}