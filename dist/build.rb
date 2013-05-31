#!/usr/local/bin/ruby -Ku
#
# Releaseバージョンを作成します。
#

require 'fileutils'
require 'make_release'
require 'kconv'
require 'mysql'
require 'rexml/document'

$HtmlBasePath = "E:/Dropbox/NicoNico/homepage/garnet-alice.net/programs/votesystem"

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
    elsif /Ragnarok\.Presentation\.Shogi\.dll$/i =~ name
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
    elsif /TimeController\.exe.*$/i =~ name
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

#
# 配布用ファイルを作成します。
#
def make_dist(appdata)
  # アセンブリバージョンが入ったディレクトリに
  # 作成ファイルを出力します。
  solution_path = File.join( File.dirname(appdata.dist_path), "VoteSystem.sln")
  appdata.build(solution_path, "CLR_V4;PUBLISHED")
  setup_dist(appdata)
  
  # zipに圧縮します。
  appdata.make_zip()
  
  # versioninfo.xmlなどを更新します。
  appdata.make_versioninfo()
  appdata.make_release_note()
  make_recent(appdata)
end

#
# wordpressのダウンロードリンクを更新します。
#
=begin
def update_link()
  con = Mysql.connect('garnet-alice.net', 'wordpress', `type db-password`, 'wordpress')
  con.charset = 'utf8'
  query = con.query('SELECT post_content FROM wp_posts WHERE ID = 120')
  if query.size != 1
    throw "query error"
  end
  
  content = query.first[0]
  puts content.kconv(Kconv::SJIS, Kconv::UTF8)
  
  doc = REXML::Document.new(content)
  puts doc
end
=end

#
# 必要なファイルをコピーします。
#
def copy_dist(appdata)
  FileUtils.copy(appdata.zip_path, File.join($HtmlBasePath, "download"))
  FileUtils.copy(appdata.versioninfo_path, File.join($HtmlBasePath, "update"))
  FileUtils.copy(appdata.releasenote_path, File.join($HtmlBasePath, "update"))
  FileUtils.copy(File.join(appdata.dist_path, "VoteClient.html"), File.join($HtmlBasePath, "download"))
end

# このスクリプトのパスは $basepath/dist/xxx.rb となっています。
dist_path = File.dirname(File.expand_path($0))

assemblyinfo_path = File.join(dist_path, "../VoteClient/Properties/AssemblyInfo.cs")
history_path = File.join(dist_path, "history.yaml")
appdata = AppData.new("votesystem", dist_path, assemblyinfo_path, history_path)

ARGV.each do |arg|
  case arg
  when "make": make_dist(appdata)
  when "copy": copy_dist(appdata)
  end
end
