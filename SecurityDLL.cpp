#include "SecurityDLL.h"
#include <string.h>

void XOROperation(const char* input, char* output, int length, const char* key) {
    int keyLength = strlen(key);
    for (int i = 0; i < length; i++) {
        output[i] = input[i] ^ key[i % keyLength];
    }
}

SECURITYDLL_API void Encrypt(const char* input, char* output, int length, const char* key) {
    XOROperation(input, output, length, key);
}

SECURITYDLL_API void Decrypt(const char* input, char* output, int length, const char* key) {
    XOROperation(input, output, length, key);
}
