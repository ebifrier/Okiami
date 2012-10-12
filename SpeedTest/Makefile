#
#
#

include ../Makefile-common

TARGET = $(OUTDIR)/SpeedTest.exe

OPT_CSFLAGS = -target:exe -define:__LINUX__
OPT_LDFLAGS = #-keyfile:VoteServer.snk
OPT_LIBS    = -r:VoteProtocol.dll -r:WindowsBase.dll

DEPEND_LIBPATH = ../Protocol

make-main:
	@gmcs -recurse:*.cs $(CSFLAGS) -out:$(TARGET) $(LDFLAGS) $(LIBS) $^
	chmod a-x $(TARGET)
