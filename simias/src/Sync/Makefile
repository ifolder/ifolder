
TARGETS := TestSync.exe SyncCmd.exe

.PHONY: all clean test simias

export MONO_PATH:=../../../lib
export LD_LIBRARY_PATH:=$(MONO_PATH)

all: $(TARGETS)

TestSync.exe: simias TestSync.cs Differ.cs
	@echo building $@; mcs -debug+ -r:Simias.dll -r:NUnit.Framework.dll -out:$@ $(filter %.cs,$^)

SyncCmd.exe: simias SyncCmd.cs
	@echo building $@; mcs -debug+ -r:Simias.dll -out:$@ $(filter %.cs,$^)

simias:
	@cd ../..; make install; cd src/Sync

test: $(TARGETS)
	mono --debug TestSync.exe 2>test.log

clean:
	@rm -rf $(TARGETS)

#===========================================================================
