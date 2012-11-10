#
#
#

include ../Makefile-common

TARGET = $(OUTDIR)/VoteProtocol.dll

OPT_CSFLAGS = -target:library
OPT_LDFLAGS = -keyfile:VoteProtocol.snk
OPT_LIBS    = -r:WindowsBase.dll

DEPEND_LIBPATH = ../../Ragnarok/Ragnarok.Shogi

make-main: $(SRCS)
	@gmcs *.cs Properties/*.cs Commenter/*.cs Vote/*.cs $(CSFLAGS) \
	-out:$(TARGET) $(LDFLAGS) $(LIBS) $^
	chmod a-x $(TARGET)
