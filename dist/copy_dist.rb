#!/usr/local/bin/ruby -Ku
#
# Releaseバージョンを作成します。
#

require 'fileutils'

$HtmlBasePath = "E:/Dropbox/NicoNico/homepage/public_html/programs/votesystem"

#
# クライアントのバージョンを取得します。
#
def get_client_version(distpath)
  filepath = File.join(distpath, "..", "Client", "Properties", "AssemblyInfo.cs")
  str = File.open(filepath).read
  
  if /\[assembly: AssemblyVersion\("([\d\.]+)"\)\]/ =~ str then
    version = $1
  else
    puts "Not found client's assembly version."
    exit(-1)
  end
end

# このスクリプトのパスは $basepath/dist/xxx.rb となっています。
distpath = File.dirname(File.expand_path($0))

# 出力ディレクトリ名
version = get_client_version(distpath)
version_ = version.gsub(".", "_")
zipname = "VoteClient_" + version_ + ".zip"

# 必要なファイルをコピーします。
FileUtils.copy(File.join(distpath, zipname), File.join($HtmlBasePath, "download"))
FileUtils.copy(File.join(distpath, "release_note.html"), File.join($HtmlBasePath, "update"))
FileUtils.copy(File.join(distpath, "versioninfo.xml"), File.join($HtmlBasePath, "update"))
