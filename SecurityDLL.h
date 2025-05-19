#pragma once

#ifdef SECURITYDLL_EXPORTS
#define SECURITYDLL_API __declspec(dllexport)
#else
#define SECURITYDLL_API __declspec(dllimport)
#endif

extern "C" {
    SECURITYDLL_API void Encrypt(const char* input, char* output, int length, const char* key);
    SECURITYDLL_API void Decrypt(const char* input, char* output, int length, const char* key);
}


