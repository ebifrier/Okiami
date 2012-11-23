﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using Ragnarok.Utility;

namespace VoteSystem.Protocol.Model
{
    using Vote;

    /// <summary>
    /// <see cref="EndRollList"/>の例外クラスです。
    /// </summary>
    public class EndRollListException : Exception
    {
        /// <summary>
        /// <see cref="XElement"/>からエラーメッセージを作成します。
        /// </summary>
        private static string MakeMessage(string message, XElement elem)
        {
            var info = (IXmlLineInfo)elem;

            return string.Format(
                "{0}行目 位置{1}: {2}",
                info.LineNumber,
                info.LinePosition,
                message);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollListException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollListException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollListException(string message, XElement elem)
            : base(MakeMessage(message, elem))
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollListException(string message, XElement elem,
                                  Exception innerException)
            : base(MakeMessage(message, elem), innerException)
        {
        }
    }

    /// <summary>
    /// 各行の部分文字列を保持します。
    /// </summary>
    public class Element
    {
        /// <summary>
        /// 文字列を取得または設定します。
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// 色を取得または設定します。
        /// </summary>
        public Color Color
        {
            get;
            set;
        }

        /// <summary>
        /// 配置する列番号を取得または設定します。
        /// </summary>
        public int Column
        {
            get;
            set;
        }

        /// <summary>
        /// 配置する列の数を取得または設定します。
        /// </summary>
        public int ColumnSpan
        {
            get;
            set;
        }

        /// <summary>
        /// 水平アラインメントを取得または設定します。
        /// </summary>
        public HorizontalAlignment HorizontalAlignment
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Element()
        {
            Color = Colors.White;
            Column = 0;
            ColumnSpan = 1;
            HorizontalAlignment = HorizontalAlignment.Center;
        }
    }

    /// <summary>
    /// 横列を指定するオブジェクトです。
    /// </summary>
    public class Column
    {
        /// <summary>
        /// 横幅比を取得または設定します。
        /// </summary>
        public double Width
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 各行文字列の情報を保持します。
    /// </summary>
    public class Line
    {
        /// <summary>
        /// 1行にある文字列のリストを取得します。
        /// </summary>
        public List<Element> ElementList
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Line()
        {
            ElementList = new List<Element>();
        }
    }

    /// <summary>
    /// エンドロール用のスタッフリストを扱います。
    /// </summary>
    public class EndRollList
    {
        private static readonly Regex propertyRegex = new Regex(
            @"[$](?:[(]([\w_]*)[)]|([\w_]*))");

        /// <summary>
        /// 列リストを取得します。
        /// </summary>
        public List<Column> ColumnList
        {
            get;
            private set;
        }

        /// <summary>
        /// 各行の情報を取得します。
        /// </summary>
        public List<Line> LineList
        {
            get;
            private set;
        }

        /// <summary>
        /// 列の幅の合計を取得します。
        /// </summary>
        public double ColumnAllWidth
        {
            get
            {
                if (ColumnList.Any())
                {
                    return 0.0;
                }

                return ColumnList.Sum(column => column.Width);
            }
        }

        /// <summary>
        /// 指定の名前のプロパティ値を取得します。
        /// </summary>
        private string GetPropertyObject(object target, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (target == null)
            {
                return value;
            }

            // プロパティ名の最初が$なら、指定の名前のプロパティから
            // 値を取得します。
            return propertyRegex.Replace(value, m =>
            {
                // $(name)で示される名前を持つプロパティを探し、
                // その値で値を置き換えます。
                var g = (m.Groups[1].Success ? m.Groups[1] : m.Groups[2]);
                var obj = (
                    g.Length > 0 ?
                    MethodUtil.GetPropertyValue(target, g.Value) :
                    target);

                return (obj == null ? "" : obj.ToString());
            });
        }

        /// <summary>
        /// ColumnDefinitionタグを処理します。
        /// </summary>
        private Column ProcessColumnDefinition(XElement elem)
        {
            try
            {
                var attr = elem.Attribute("Width");
                if (attr == null)
                {
                    throw new EndRollListException(
                        "ColumnDefinitionタグ: Width属性がありません。", elem);
                }

                var width = double.Parse(attr.Value);
                return new Column() { Width = width };
            }
            catch (Exception ex)
            {
                throw new EndRollListException(
                    "ColumnDefinitionタグの解析に失敗しました。", elem, ex);
            }
        }

        /// <summary>
        /// ColumnDefinitionsタグを処理します。
        /// </summary>
        private List<Column> ProcessColumnDefinitions(XElement elem)
        {
            return elem.Elements().Select(elemChild =>
            {
                if (elemChild.Name.LocalName == "ColumnDefinition")
                {
                    return ProcessColumnDefinition(elemChild);
                }
                else
                {
                    throw new EndRollListException(
                        string.Format("{0}タグには対応していません。",
                            elemChild.Name.LocalName),
                        elemChild);
                }
            }).ToList();
        }

        /// <summary>
        /// xaml用の色を返します。
        /// </summary>
        private static Color GetXamlColor(Protocol.NotificationColor color)
        {
            switch (color)
            {
                case NotificationColor.Default:
                    return Colors.White;
                case NotificationColor.Black:
                    return Colors.Black;
                case NotificationColor.White:
                    return Colors.White;
                case NotificationColor.Red:
                    return Colors.Red;
                case NotificationColor.Pink:
                    return Colors.Pink;
                case NotificationColor.Orange:
                    return Colors.Orange;
                case NotificationColor.Yellow:
                    return Colors.Goldenrod;
                case NotificationColor.Green:
                    return Colors.Green;
                case NotificationColor.Cyan:
                    return Colors.Cyan;
                case NotificationColor.Blue:
                    return Colors.LightBlue;
                case NotificationColor.Purple:
                    //return Colors.Purple;
                    return Colors.Magenta;
            }

            return Colors.Black;
        }

        /// <summary>
        /// Formatタグを処理します。
        /// </summary>
        /// <remarks>
        /// 複数のFormatタグが１行を形成します。
        /// </remarks>
        private Element ProcessFormatTag(XElement elem, object data)
        {
            try
            {
                var result = new Element();
                var value = string.Empty;

                var attr = elem.Attribute("Text");
                if (attr != null)
                {
                    result.Text = GetPropertyObject(data, attr.Value);
                }

                attr = elem.Attribute("Color");
                if (attr != null)
                {
                    value = GetPropertyObject(data, attr.Value);

                    if (Enum.IsDefined(typeof(NotificationColor), value))
                    {
                        // NotificationColorの色ならそれを使います。
                        var notificationColor = (NotificationColor)
                            Enum.Parse(typeof(NotificationColor), value);

                        result.Color = GetXamlColor(notificationColor);
                    }
                    else
                    {
                        // xaml の色のはず(!?)です。
                        result.Color = (Color)
                            System.Windows.Media.ColorConverter.ConvertFromString(value);
                    }
                }

                attr = elem.Attribute("Column");
                if (attr != null)
                {
                    result.Column = int.Parse(attr.Value);
                }

                attr = elem.Attribute("ColumnSpan");
                if (attr != null)
                {
                    result.ColumnSpan = int.Parse(attr.Value);
                }

                attr = elem.Attribute("Alignment");
                if (attr != null)
                {
                    value = GetPropertyObject(data, attr.Value);

                    result.HorizontalAlignment = (HorizontalAlignment)
                        Enum.Parse(typeof(HorizontalAlignment), value);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new EndRollListException(
                    "Formatタグの解析に失敗しました。", elem, ex);
            }
        }

        /// <summary>
        /// Lineタグを処理します。
        /// </summary>
        private Line ProcessLineTag(XElement elem, object data)
        {
            return new Line()
            {
                ElementList = elem.Elements().Select(elemChild =>
                {
                    if (elemChild.Name.LocalName == "Format")
                    {
                        return ProcessFormatTag(elemChild, data);
                    }
                    else
                    {
                        throw new EndRollListException(
                            string.Format("{0}タグには対応していません。",
                                elemChild.Name.LocalName),
                            elemChild);
                    }
                }).ToList(),
            };
        }

        /// <summary>
        /// 情報を持つデータオブジェクトを取得します。
        /// </summary>
        private List<object> GetDataObject(VoterList voterList, string propertyName)
        {
            var m = propertyRegex.Match(propertyName);
            if (!m.Success)
            {
                return null;
            }

            var g = (m.Groups[1].Success ? m.Groups[1] : m.Groups[2]);
            switch (g.Value)
            {
                case "UnjoinedVoterCount":
                    return new List<object>() { voterList.UnjoinedVoterCount };
                case "LiveOwner":
                    return new List<object>() { /*Global.MainModel.NickName*/ };
                    
                case "JoinedVoterList":
                    return voterList.JoinedVoterList.ToList<object>();
                case "LiveOwnerList":
                    return voterList.LiveOwnerList.ToList<object>();
                case "ModeCustomJoinerList":
                    return voterList.ModeCustomJoinerList.ToList<object>();
            }

            return null;
        }

        /// <summary>
        /// Linesタグを処理します。
        /// </summary>
        /// <remarks>
        /// タグの中に複数行の情報が入っています。
        /// これをデータ数と同じ行数に展開します。
        /// </remarks>
        private IEnumerable<Line> ProcessLinesTag(XElement elem, VoterList voterList)
        {
            var dataAttr = elem.Attribute("Data");
            if (dataAttr == null)
            {
                throw new EndRollListException(
                    "LineFormatタグにData属性がありません。", elem);
            }

            var dataList = GetDataObject(voterList, dataAttr.Value);
            if (dataList == null)
            {
                throw new EndRollListException(
                    "Data属性の値が正しくありません。", elem);
            }

            var lines = elem.Elements();

            // 各行の情報をデータの数分だけ作成します。
            return dataList.SelectMany(data =>
                lines.Select(line =>
                    {
                        if (line.Name.LocalName != "Line")
                        {
                            throw new EndRollListException(
                                string.Format(
                                    "{0}タグには対応していません。",
                                    line.Name.LocalName),
                                line);
                        }

                        return ProcessLineTag(line, data);
                    }));
        }

        /// <summary>
        /// xmlファイルを読み込みます。
        /// </summary>
        public void Load(string filename, VoterList voterList)
        {
            var doc = XElement.Load(filename, LoadOptions.SetLineInfo);
            var elemList = doc.Elements();
            var columnList = this.ColumnList; // 列オブジェクトのデフォルト値。

            // Lines タグを処理していきます。
            var result = new List<Line>();
            foreach (var elem in elemList)
            {
                if (elem.Name.LocalName == "ColumnDefinitions")
                {
                    columnList = ProcessColumnDefinitions(elem);
                }
                else if (elem.Name.LocalName == "Line")
                {
                    result.Add(ProcessLineTag(elem, null));
                }
                else if (elem.Name.LocalName == "Lines")
                {
                    result.AddRange(ProcessLinesTag(elem, voterList));
                }
                else
                {
                    throw new EndRollListException(
                        string.Format("{0}タグには対応していません。",
                            elem.Name.LocalName),
                        elem);
                }
            }

            // 解析に成功したら、値を設定します。
            this.ColumnList = columnList;
            this.LineList = result;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollList()
        {
            this.ColumnList = new List<Column>()
            {
                new Column(){ Width = 1.0 },
            };
        }
    }
}