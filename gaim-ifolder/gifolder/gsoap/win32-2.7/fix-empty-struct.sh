#!/bin/sh
sed '
/^{$/ {
N
/\n};$/ {
# found it - now replace
s/{.*\n.*};/{\
	void *dummy;\
};/
}
}' $1

