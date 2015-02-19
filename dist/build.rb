#!/usr/local/bin/ruby -Ku
#
# Releaseバージョンを作成します。
#

require 'fileutils'
require 'make_release'

#
# ビルドしたファイルを配布用ディレクトリにコピーします。
#
def setup_dist(appdata)
  outdir = appdata.outdir_path
  FileUtils.mkdir(File.join(outdir, "Plugin"))
  
  Dir.glob(File.join(outdir, "*")).each do |name|
    if /\.(pdb|xml)$/i =~ name or
       /nunit\.framework\./ =~ name or
       /VoteServer\.exe.*$/i =~ name or
       /SpeedTest\.exe.*$/i =~ name or
       /TimeController\.exe.*$/i =~ name
      deleteall(name)
    elsif /Plugin\w*\.dll$/i =~ name or
          /Ragnarok.*\.Shogi\.dll$/i =~ name or
          /ShogiData$/i =~ name
      FileUtils.mv(name, File.join(outdir, "Plugin"))
    end
  end
  
  # readme.txt をコピーします。
  FileUtils.cp("readme.html", outdir)
end

#
# 配布用ファイルを作成します。
#
def make_dist(appdata)
  define = "CLR4_0;CLR_GE_2_0;CLR_GE_3_0;CLR_GE_3_5;CLR_GE_4_0;PUBLISHED"

  # アセンブリバージョンが入ったディレクトリに
  # 作成ファイルを出力します。
  solution_path = File.join(File.dirname(appdata.dist_path), "VoteSystem.sln")
  appdata.build(solution_path, define)
  setup_dist(appdata)
  
  # zipに圧縮します。
  appdata.make_zip()
end

#
# 必要なファイルをコピーします。
#
def copy_dist(appdata)
  FileUtils.copy(
    appdata.zip_path,
    File.join(appdata.local_base_path, "download"))
  FileUtils.copy(
    appdata.history_path,
    File.join($LocalBasePath, "../data/okiami.yml"))
end

# このスクリプトのパスは $basepath/dist/xxx.rb となっています。
dist_path = File.dirname(File.expand_path($0))

assemblyinfo_path = File.join(dist_path, "../VoteClient/Properties/AssemblyInfo.cs")
appdata = AppDataForBuild.new(dist_path, assemblyinfo_path)

make_dist(appdata)
copy_dist(appdata)
