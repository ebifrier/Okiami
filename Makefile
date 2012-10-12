#
#
#

SUBDIRS = Server

all:
	@for d in $(SUBDIRS); do \
		(cd $$d; make all) \
	done

hg:
	(cd ../Ragnarok; make hg)
	hg pull
	hg update

clean:
	@for d in $(SUBDIRS); do \
		(cd $$d; make clean) \
	done
