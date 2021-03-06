﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ShareInvest.AutoMessageBox;
using ShareInvest.EventHandler;
using ShareInvest.Identify;

namespace ShareInvest.Secret
{
    public class Conceal
    {
        private readonly Dictionary<string, string> unique = new Dictionary<string, string>()
        {
            { "jhy7264", "주식회사공유인베" },
            { "share9", "주식회사공유인베" },
            { "share915", "박상우" },
            { "prophet8", "전혜영" },
            { "prophet9", "박상우" }
        };
        private int RecentDate
        {
            get; set;
        }
        protected readonly string[] unique_account =
        {
            "2770591531",
            "8744695931",
            "8744158731",
            "8744131431",
            "8743861631",
            "8744716231",
            "8744725631",
            "8744760731",
            "8744791131",
            "8744791231",
            "8744791331",
            "5019625231"
        };
        protected bool Identify(string id, string name)
        {
            confirm = string.Concat(id.Substring(0, 1).ToUpper(), id.Substring(1));

            return unique.ContainsKey(id) && unique.ContainsValue(name);
        }
        protected string SetSecret(int type)
        {
            string path = type > 0 ? @"\Statistics\Kosdaq150" : @"\Statistics\Kospi200";
            string[] file = Directory.GetFiles(string.Concat(Environment.CurrentDirectory, path), "*.csv", SearchOption.AllDirectories), arr;
            int temp;

            foreach (string val in file)
            {
                arr = val.Split('.');
                arr = arr[0].Split('\\');
                temp = int.Parse(arr[arr.Length - 1]);

                if (temp > RecentDate)
                    RecentDate = temp;
            }
            try
            {
                using (StreamReader sr = new StreamReader(string.Concat(Environment.CurrentDirectory, path, @"\", RecentDate.ToString(), ".csv")))
                {
                    List<string> list = new List<string>(128);

                    if (sr != null)
                        while (sr.EndOfStream == false)
                            list.Add(sr.ReadLine());

                    Dictionary<string, long> max = new Dictionary<string, long>(128);
                    file = list[0].Split(',');
                    arr = list[list.Count - 1].Split(',');

                    for (temp = 1; temp < file.Length - 1; temp++)
                        max[file[temp]] = long.Parse(arr[temp]);

                    using (Choice ch = new Choice())
                    {
                        SendText += ch.OnReceiveText;

                        foreach (KeyValuePair<string, long> kv in max.OrderByDescending(o => o.Value))
                        {
                            if (ch.Count > 29)
                                break;

                            SendText.Invoke(this, new Memorize(string.Concat(kv.Key, " ￦", kv.Value.ToString("N0"))));
                        }
                        ch.ShowDialog();
                        Box.Show(string.Concat(ch.TempText), "The Chosen Strategy is Number. . .", waiting);
                        arr = ch.TempText.Split('￦');
                        ch.Dispose();

                        return arr[0].Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return "1^1";
        }
        protected StringBuilder sb;
        protected string confirm;
        protected string account;
        protected int screen;
        protected const string it = "Information that already Exists";
        protected const string pt = "ProhibitTrading";
        protected const int waiting = 3500;
        protected const int delay = 205;
        protected const int end = 1;
        protected const int tm = 250000;
        protected const int ktm = 10000;
        protected const double commission = 3e-5;
        protected const double margin = 7.65e-2;
        protected const double kqm = 1.665e-1;
        protected const double value = 5e-2;
        public event EventHandler<Memorize> SendText;
    }
}