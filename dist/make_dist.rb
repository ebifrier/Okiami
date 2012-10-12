#!/usr/local/bin/ruby -Ku
#
# Releaseバージョンを作成します。
#

require 'rubygems'
require 'yaml'
require 'fileutils'
require 'image_size'
require 'net/ftp'
require 'digest/md5'
require 'rexml/document'

$HtmlBasePath = "E:/Dropbox/NicoNico/homepage/public_html/programs/votesystem"

#
# 各リリース情報を保持します。
#
class ReleaseData
  attr_accessor :version, :thumbnail, :date, :content

  def initialize(node)
    @version = node["version"]

    @thumbnail = node["thumbnail"]
    @date = node["date"]
    @content = node["content"]
  end

  # infoタグをhtml表示用に変換したcontentを取得します。
  def html_content()
    root = REXML::Element.new("ul")

    @content.each do |info_node|
      new_node = REXML::Element.new("li")

      if info_node.instance_of?(String)
        # 文字列のみの場合は、それを直接設定。
        new_node.text = info_node
      else
        pair = info_node.first

        # infoのタイプ名です。
        text = "(" + pair[0] + ") "

        if pair[0] == "本体"
          new_node.add_element("span", {"class" => "main"}).add_text(text)
        elsif pair[0] == "将棋"
          new_node.add_element("span", {"class" => "shogi"}).add_text(text)
        else
          new_node.add_element("span", {"class" => "unknown"}).add_text(text)
        end
        
        new_node.add_text(pair[1])
      end
      root.add_element(new_node)
    end
    
    root
  end
end

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

#
# ファイル又はディレクトリを削除するメソッドを作成
#
def deleteall(delthem)
  if FileTest.directory?(delthem) then  # ディレクトリかどうかを判別
    Dir.foreach( delthem ) do |file|    # 中身を一覧
      next if /^\.+$/ =~ file           # 上位ディレクトリと自身を対象から外す
      deleteall(delthem.sub(/\/+$/,"") + "/" + file)
    end
    Dir.rmdir(delthem) rescue ""        # 中身が空になったディレクトリを削除
  else
    if File.exists?(delthem)
      File.delete(delthem)                # ディレクトリでなければ削除
    end
  end
end

#
# プロジェクトのリビルドを行います。
#
def build(distpath, output)
  sln = File.join(File.dirname(distpath), "VoteSystem.sln")
  
  # ディレクトリが存在したら削除します。
  if File.exists?(output)
    deleteall(output)
  end
  
  # ビルドコマンドを実行します。
  path_command = "call e:\\Dropbox\\bin\\path-vc10"
  build_command =
    "msbuild /nologo \"#{sln}\" /t:Rebuild /p:DefineConstants=\"CLR_V4;PUBLISHED\" " +
    "/p:OutputPath=\"#{output}\" /p:Configuration=Release"
  
  puts build_command
  system(path_command + " && " + build_command)
end

#
# ビルドしたファイルを配布用ディレクトリにコピーします。
#
def setup_dist(output)
  Dir::mkdir(File.join(output, "Dll"))
  Dir::mkdir(File.join(output, "Plugin"))
  
  Dir.glob(File.join(output, "*")).each do |name|
    if /\.(pdb|xml)$/i =~ name
      deleteall(name)
    elsif /Plugin\w*\.dll$/i =~ name
      FileUtils.mv(name, File.join(output, "Plugin", File.basename(name)))
    elsif /(\.dll)|(bg|de|es|fr|it|ja|lt|nl|pt|ru|zh)([-]\w+)?$/i =~ name
      FileUtils.mv(name, File.join(output, "Dll", File.basename(name)))
    elsif File::ftype(name) == "directory" and
          /(bg|de|es|fr|it|ja|lt|nl|pt|ru|zh)([-]\w+)?$/i =~ name
      FileUtils.mv(name, File.join(output, "Dll", File.basename(name)))
    elsif /VoteServer\.exe.*$/i =~ name
      deleteall(name)
    elsif /SpeedTest\.exe.*$/i =~ name
      deleteall(name)
    end
  end
  
  # readme.txt をコピーします。
  FileUtils.cp("readme.txt", File.join(output, "readme.txt"))
end

#
# zipファイルを作成します。
#
def make_zip(dirname, zippath)
  deleteall(zippath)

  Dir.chdir(dirname)
  filelist = Dir.glob("*").map do |name|
    '"' + name + '"'
  end
  files_str = filelist.join(' ')

  zip_command = "zip -r -q \"#{zippath}\" " + files_str
  printf("begin command: %s\n", zip_command)
  system(zip_command)

  Dir.chdir("..")
end

#
# ファイルのMD5を計算します。
#
def compute_md5(filename)
  # binary mode必須
  File.open(filename, "rb") do |f|
    s = f.read

    return Digest::MD5.hexdigest(s)
  end
end

#
# versioninfo.xmlを更新します。
#
def make_versioninfo(version, zipname)
  input_path = File.join("versioninfo_templ.xml")
  output_path = File.join("versioninfo.xml")
  
  data = File.open(input_path).read
  data = data.gsub("${VERSION}", version)
  data = data.gsub("${_VERSION}", version.gsub(".", "_"))
  data = data.gsub("${ZIP_FILE}", zipname)
  data = data.gsub("${MD5}", compute_md5(zipname))
  data = data.gsub("${PUB_DATE}", Time.now.to_s)
  data = data.gsub("${RAND}", rand(100000).to_s)
  
  File.open(output_path, "w") do |f|
    f.write(data)
    printf("wrote %s\n", output_path)
  end
end

#
# リリース情報をyamlファイルから読み込みます。
#
def load_release_data()
  history = YAML.load_file("history.yaml")

  history.map do |node|
    ReleaseData.new(node)
  end
end

#
# サーバー上のサムネを一つ、バージョンと日付から選択します。
#
def select_thumbnail(rdata)
  return rdata.thumbnail if rdata.thumbnail != nil

  image_path = File.join($HtmlBasePath, "..", "..", "common", "images")
  filelist = Dir.glob(File.join(image_path, "*")).
    map do |path| File.basename(path) end.
    sort

  # md5から一意な画像を選択します。
  md5 = Digest::MD5.new()
  md5.update(rdata.version)
  md5.update(rdata.date)
  message = md5.digest

  digest = 0
  message.bytes do |b|
    digest += b
  end

  filelist[digest % filelist.length]
end

#
# サーバー上のサムネイルのフルパスを取得します。
#
def make_thumbnail_fullpath(filename)
  "http://garnet-alice.net/common/images/" + filename
end

#
# 画像のサイズを取得します。
#
def get_image_size(filename)
  image_dir = File.join($HtmlBasePath, "..", "..", "common", "images")

  File::open(File.join(image_dir, filename), "rb") do |f|
    return ImageSize.new(f.read)
  end
end

#
# リリース情報(html)を出力します。
#
def write_release_note(version)
  output = File.join("release_note.html")

  # release_note.html のテンプレートファイルを読み込みます。
  fp = File.open("release_note_templ.html", "r")
  doc = REXML::Document.new(fp)

  top = doc.elements["//body"]
  rnote_templ = top.elements["//div[@class=\"versioninfo\"]"]

  # 更新情報を読み込みます。
  data_list = load_release_data()

  # delete_allは実体を消すためコピーが必要になります。
  rnote_templ = rnote_templ.deep_clone
  top.elements.delete_all("*")

  data_list.each_with_index do |rdata, i|
    break if i >= 3
    note_node = rnote_templ.deep_clone

    if i == 0 and rdata.version != version
      raise StandardError, 'Release version isn\'t matched (history.xml)'
    end

    # header
    node = note_node.elements["//*[@class=\"version\"]"]
    node.text = "更新履歴　VoteClient " + rdata.version

    # thumbnail
    node = note_node.elements["//*[@class=\"thumbnail\"]"]
    filename = select_thumbnail(rdata)
    image_size = get_image_size(filename)
    attrs = {
      'src' => make_thumbnail_fullpath(filename),
      'alt' => '更新情報',
      'border' => '0',
      'width' => image_size.width,
      'height' => image_size.height,
    }
    node.add_element("img", attrs)
    select_thumbnail(rdata)

    # date
    node = note_node.elements["//*[@class=\"date\"]"]
    node.text = (rdata.date ? "更新日: " + rdata.date : " ")

    # content
    node = note_node.elements["//*[@class=\"content\"]"]
    node.add_element(rdata.html_content)

    top.add_element(note_node)
  end

  File.open(output, "w") do |f|
    doc.write(f, -1, false, true)
  end
end


# このスクリプトのパスは $basepath/dist/xxx.rb となっています。
distpath = File.dirname(File.expand_path($0))

# 出力ディレクトリ名
version = get_client_version(distpath)
version_ = version.gsub(".", "_")
dirname = "VoteClient_" + version_
output = File.join(distpath, dirname)

zipname = dirname + ".zip"
zippath = File.join(distpath, zipname)

# アセンブリバージョンが入ったディレクトリに
# 作成ファイルを出力します。
build(distpath, output)
setup_dist(output)

# zipに圧縮します。
make_zip(dirname, zippath)

# versioninfo.xmlを更新します。
make_versioninfo(version, zipname)
write_release_note(version)
