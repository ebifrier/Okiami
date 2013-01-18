#!/usr/local/bin/ruby -Ku
#
# Releaseバージョンを作成します。
#

require 'fileutils'
require 'make_release'

$RagnarokUpdatePath = "..\\..\\Ragnarok\\Ragnarok\\Update"
                               
#
# ビルドしたファイルを配布用ディレクトリにコピーします。
#
def setup_dist(appdata)
  outdir = appdata.outdir_path
  Dir::mkdir(File.join(outdir, "Dll"))
  Dir::mkdir(File.join(outdir, "Plugin"))
  
  Dir.glob(File.join(outdir, "*")).each do |name|
    if /\.(pdb|xml)$/i =~ name
      deleteall(name)
    elsif /nunit\.framework\./ =~ name
      deleteall(name)
    elsif /Plugin\w*\.dll$/i =~ name
      FileUtils.mv(name, File.join(outdir, "Plugin", File.basename(name)))
    elsif /(\.dll)|(bg|de|es|fr|it|ja|lt|nl|pt|ru|zh)([-]\w+)?$/i =~ name
      FileUtils.mv(name, File.join(outdir, "Dll", File.basename(name)))
    elsif File::ftype(name) == "directory" and
          /(bg|de|es|fr|it|ja|lt|nl|pt|ru|zh)([-]\w+)?$/i =~ name
      FileUtils.mv(name, File.join(outdir, "Dll", File.basename(name)))
    elsif /VoteServer\.exe.*$/i =~ name
      deleteall(name)
    elsif /SpeedTest\.exe.*$/i =~ name
      deleteall(name)
    end
  end
  
  # readme.txt をコピーします。
  FileUtils.cp("readme.txt", File.join(outdir, "readme.txt"))
end

#
# VoteClient.htmlを更新します。
#
def make_recent(appdata)
  input_path = File.join("VoteClient_templ.html")
  output_path = File.join("VoteClient.html")

  appdata.convert_template(input_path, output_path)
end

# このスクリプトのパスは $basepath/dist/xxx.rb となっています。
dist_path = File.dirname(File.expand_path($0))

assemblyinfo_path = File.join(dist_path,
  "..", "VoteClient", "Properties", "AssemblyInfo.cs")
history_path = File.join(dist_path, "history.yaml")
appdata = AppData.new("votesystem", dist_path, assemblyinfo_path, history_path)

# アセンブリバージョンが入ったディレクトリに
# 作成ファイルを出力します。
solution_path = File.join(File.dirname(dist_path), "VoteSystem.sln")
appdata.build(solution_path, "CLR_V4;PUBLISHED")
setup_dist(appdata)

# zipに圧縮します。
appdata.make_zip()

# versioninfo.xmlを更新します。
versioninfo_tmpl = File.join($RagnarokUpdatePath, "versioninfo_tmpl.xml")
releasenote_tmpl = File.join($RagnarokUpdatePath, "release_note_tmpl.html")

appdata.make_versioninfo(versioninfo_tmpl)
make_recent(appdata)
appdata.make_release_note(releasenote_tmpl)
