TARGET_TYPE=exe
TARGET_SYMBOLS_TYPE=exe.mdb
TARGET_NAME=Hasher
TARGET=$(TARGET_NAME).$(TARGET_TYPE)
TARGET_SYMBOLS=$(TARGET_NAME).$(TARGET_SYMBOLS_TYPE)



.SHELLFLAGS=-x

SHELL=/bin/bash


BIN_DIR=bin/Debug

XBUILD_FLAGS=\
    /p:XBuild=true\
    /verbosity:diag


$(BIN_DIR)/$(TARGET): 
	xbuild $(XBUILD_FLAGS)


clean:
	xbuild $(XBUILD_FLAGS) /target:clean


install: $(TARGET_NAME) $(BIN_DIR)/$(TARGET) $(BIN_DIR)/$(TARGET_SYMBOLS)
	p4 edit $(MACHINESROOT)/common/bin/$(TARGET_NAME) $(MACHINESROOT)/common/bin/PC/$(TARGET) $(MACHINESROOT)/common/bin/PC/$(TARGET_SYMBOLS)
	cp $(TARGET_NAME) $(MACHINESROOT)/common/bin/
	cp $(BIN_DIR)/$(TARGET) $(MACHINESROOT)/common/bin/PC/
	cp $(BIN_DIR)/$(TARGET_SYMBOLS) $(MACHINESROOT)/common/bin/PC/


all: clean $(BIN_DIR)/$(TARGET) install
