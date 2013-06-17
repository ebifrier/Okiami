using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Reflection;

using Ragnarok;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Client
{
    using VoteSystem.Protocol;

    /// <summary>
    /// ホストからプラグイン側に与えられるオブジェクトです。
    /// </summary>
    public sealed class PluginHost : MarshalByRefObject
    {
        /// <summary>
        /// 実行ファイルのあるパスを取得します。
        /// </summary>
        public string ExecutePath
        {
            get;
            internal set;
        }

        /// <summary>
        /// 設定ファイルのあるディレクトリパスを取得します。
        /// </summary>
        public string SettingDir
        {
            get;
            internal set;
        }

        /// <summary>
        /// メインウィンドウを取得または設定します。
        /// </summary>
        public View.MainWindow Window
        {
            get;
            internal set;
        }

        /// <summary>
        /// 通信用オブジェクトを取得します。
        /// </summary>
        public Model.VoteClient VoteClient
        {
            get;
            internal set;
        }

        /// <summary>
        /// クライアントのメインオブジェクトを取得します。
        /// </summary>
        public Model.MainModel MainModel
        {
            get;
            internal set;
        }
    }

    /// <summary>
    /// プラグイン用のクラスです。
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// プラグイン名を取得します。
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// プラグインの初期化を行います。
        /// </summary>
        void Initialize(PluginHost host);

        /// <summary>
        /// コマンドの実行を行います。
        /// </summary>
        void Run();

        /// <summary>
        /// 処理ハンドラを接続します。
        /// </summary>
        void ConnectHandlers(PbConnection connection);

        /// <summary>
        /// 受信した通知を処理します。
        /// </summary>
        void HandleNotification(Notification notification);
    }

    /// <summary>
    /// プラグインを使うためのユーティリティクラスです。
    /// </summary>
    internal static class PluginUtil
    {
        /// <summary>
        /// プラグインを読み込み、オブジェクトを作成します。
        /// </summary>
        internal static IPlugin LoadPlugin(Type pluginType)
        {
            try
            {
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                if (plugin == null)
                {
                    return null;
                }

                // 実行ファイルのパスなどを取得します。
                var executeAsm = Assembly.GetEntryAssembly();
                var conf = ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.PerUserRoamingAndLocal);

                // ホスト側の情報を設定します。
                plugin.Initialize(new PluginHost()
                {
                    ExecutePath = executeAsm.Location,
                    SettingDir = Path.GetDirectoryName(conf.FilePath),
                    Window = Global.MainWindow,
                    VoteClient = Global.VoteClient,
                    MainModel = Global.MainModel,
                });

                return plugin;
            }
            catch (BadImageFormatException)
            {
                // ネイティブdllを読み込んだ場合。
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "'{0}': プラグインの読み込みに失敗しました。",
                    pluginType);
            }

            return null;
        }

        /// <summary>
        /// プラグインを読み込み、オブジェクトを作成します。
        /// </summary>
        internal static IPlugin LoadPlugin(string dllPath)
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(dllPath);
                var asm = Assembly.LoadFrom(dllPath);
                var types = asm.GetExportedTypes();

                Log.Trace(name.Name + " を読み込み中");

                // IPluginを継承した型を検索します。
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsPublic && !type.IsAbstract &&
                        type.GetInterface(typeof(IPlugin).FullName) != null)
                    {
                        return LoadPlugin(type);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.FatalException(ex,
                    "プラグインの読み込みに失敗しました。");
            }

            return null;
        }

        /// <summary>
        /// プラグインを読み込みます。
        /// </summary>
        internal static List<IPlugin> LoadPlugins()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var pluginPath = Path.Combine(
                    Path.GetDirectoryName(asm.Location),
                    "Plugin");
                if (!Directory.Exists(pluginPath))
                {
                    return new List<IPlugin>();
                }

                // Plugin/xxx.dllからプラグインを読み込みます。
                return Directory
                    .EnumerateFiles(pluginPath, "*.dll")
                    .Where(_ => Path.GetFileName(_).StartsWith("Plugin"))
                    .Select(_ => LoadPlugin(_))
                    .Where(plugin => plugin != null)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.FatalException(ex,
                    "プラグインの読み込みに失敗しました。");
            }

            return new List<IPlugin>();
        }
    }
}
