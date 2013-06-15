#!/usr/local/bin/ruby -Ku
#
# Releaseバージョンを作成します。
#

require 'fileutils'
require 'make_release'

$HtmlBasePath = "E:/Dropbox/NicoNico/homepage/garnet-alice.net/programs/timecontroller"

#
# ビルドしたファイルを配布用ディレクトリにコピーします。
#
def setup_dist(appdata)
  outdir = appdata.outdir_path
  
  Dir.glob(File.join(outdir, "*")).each do |name|
    if /\.(pdb|xml)$/i =~ name
      deleteall(name)
    elsif /nunit\.framework\./ =~ name
      deleteall(name)
    end
  end
  
  # readme.txt をコピーします。
  FileUtils.cp("readme.html", File.join(outdir, "readme.html"))
end

#
# VoteClient.htmlを更新します。
#
def make_recent(appdata)
  input_path = File.join("TimeController_templ.html")
  output_path = File.join("TimeController.html")

  appdata.convert_template(input_path, output_path)
end

#
# 配布用ファイルを作成します。
#
def make_dist(appdata)
  # アセンブリバージョンが入ったディレクトリに
  # 作成ファイルを出力します。
  solution_path = File.join( File.dirname(appdata.dist_path), "TimeController.csproj")
  appdata.build(solution_path, "CLR_V4")
  setup_dist(appdata)
  
  # zipに圧縮します。
  appdata.make_zip()
  make_recent(appdata)
end

#
# 必要なファイルをコピーします。
#
def copy_dist(appdata)
  FileUtils.copy(appdata.zip_path, $HtmlBasePath)
  FileUtils.copy(File.join(appdata.dist_path, "TimeController.html"), $HtmlBasePath)
end

# このスクリプトのパスは $basepath/dist/xxx.rb となっています。
dist_path = File.dirname(File.expand_path($0))

assemblyinfo_path = File.join(dist_path, "../Properties/AssemblyInfo.cs")
appdata = AppData.new("timecontroller", dist_path, assemblyinfo_path, nil)

ARGV.each do |arg|
  case arg
  when "make": make_dist(appdata)
  when "copy": copy_dist(appdata)
  end
end
