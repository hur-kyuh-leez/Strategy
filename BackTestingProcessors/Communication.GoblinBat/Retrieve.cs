﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ShareInvest.Communication
{
    public class Retrieve : IFetch
    {
        private List<string> ReadCSV(string file, List<string> list)
        {
            try
            {
                using StreamReader sr = new StreamReader(file);
                if (sr != null)
                    while (sr.EndOfStream == false)
                        list.Add(sr.ReadLine());
            }
            catch (Exception ex)
            {
                list.Add(ex.ToString());
            }
            return list;
        }
        private Retrieve()
        {
            DayChart = ReadCSV(Array.Find(Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, @"..\"), "*.csv", SearchOption.AllDirectories), o => o.Contains("Day")), DayChart);
            TickChart = ReadCSV(Array.Find(Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, @"..\"), "*.csv", SearchOption.AllDirectories), o => o.Contains("Tick")), TickChart);
        }
        public static Retrieve Get()
        {
            if (retrieve == null)
                retrieve = new Retrieve();

            return retrieve;
        }
        public List<string> DayChart
        {
            get; private set;
        } = new List<string>(256);
        public List<string> TickChart
        {
            get; private set;
        } = new List<string>(2097152);
        private static Retrieve retrieve;
    }
}