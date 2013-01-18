#!/usr/local/bin/ruby -Ku
#
# Releaseバージョンを作成します。
#

require 'fileutils'
require 'make_release'

$HtmlBasePath = "E:/Dropbox/NicoNico/homepage/public_html/programs/votesystem"

# このスクリプトのパスは $basepath/dist/xxx.rb となっています。
dist_path = File.dirname(File.expand_path($0))

# 出力ディレクトリ名
assemblyinfo_path = File.join(dist_path,
  "..", "VoteClient", "Properties", "AssemblyInfo.cs")
appdata = AppData.new("votesystem", dist_path, assemblyinfo_path, nil)

# 必要なファイルをコピーします。
FileUtils.copy(appdata.zip_path, File.join($HtmlBasePath, "download"))
FileUtils.copy(appdata.versioninfo_path, File.join($HtmlBasePath, "update"))
FileUtils.copy(appdata.releasenote_path, File.join($HtmlBasePath, "update"))
FileUtils.copy(File.join(dist_path, "VoteClient.html"), File.join($HtmlBasePath, "download"))
