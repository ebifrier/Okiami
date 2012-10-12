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
	@gmcs -recurse:*.cs $(CSFLAGS) -out:$(TARGET) $(LDFLAGS) $(LIBS) $^
	chmod a-x $(TARGET)
