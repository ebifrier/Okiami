using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

#if !MONO
using System.Windows.Markup;
#endif

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("VoteProtocol")]
[assembly: AssemblyDescription("投票ツールのサーバー/クライアントの共用ライブラリです。")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("co516151")]
[assembly: AssemblyProduct("VoteProtocol")]
[assembly: AssemblyCopyright("Copyright © えびふらい 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから 
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// 次の GUID は、このプロジェクトが COM に公開される場合の、typelib の ID です
[assembly: Guid("3ddba66d-3c78-4610-b48f-fe8e25d0e367")]

#if !MONO
[assembly: ThemeInfo(
    //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.None,

    //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly
)]

[assembly: XmlnsDefinition("http://schemas.garnet-alice.net/votesystem/xaml/presentation", "VoteSystem.Protocol")]
[assembly: XmlnsDefinition("http://schemas.garnet-alice.net/votesystem/xaml/presentation", "VoteSystem.Protocol.Commenter")]
[assembly: XmlnsDefinition("http://schemas.garnet-alice.net/votesystem/xaml/presentation", "VoteSystem.Protocol.Model")]
[assembly: XmlnsDefinition("http://schemas.garnet-alice.net/votesystem/xaml/presentation", "VoteSystem.Protocol.View")]
[assembly: XmlnsDefinition("http://schemas.garnet-alice.net/votesystem/xaml/presentation", "VoteSystem.Protocol.Vote")]
[assembly: XmlnsDefinition("http://schemas.garnet-alice.net/votesystem/xaml/presentation", "VoteSystem.Protocol.Xaml")]
#endif

// アセンブリのバージョン情報は、以下の 4 つの値で構成されています:
//
//      Major PbProtocolVersion
//      Minor PbProtocolVersion 
//      Build Number
//      Revision
//
// すべての値を指定するか、下のように '*' を使ってビルドおよびリビジョン番号を 
// 既定値にすることができます:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
//[assembly: AssemblyFileVersion("1.0.0.0")]
