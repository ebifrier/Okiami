#
#
#

include ../Makefile-common

TARGET = $(OUTDIR)/VoteServer.exe

OPT_CSFLAGS = -target:exe -define:__LINUX__ -define:MONO
OPT_LDFLAGS = -keyfile:VoteServer.snk
OPT_LIBS    = -r:Mono.Posix.dll -r:VoteProtocol.dll

DEPEND_LIBPATH = ../Protocol

make-main:
	@gmcs -recurse:*.cs $(CSFLAGS) -out:$(TARGET) $(LDFLAGS) $(LIBS) $^
	@cp *.nlog $(OUTDIR)
	chmod a-x $(TARGET)
