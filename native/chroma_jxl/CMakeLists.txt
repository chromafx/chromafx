cmake_minimum_required(VERSION 3.16)
project(chromajxl LANGUAGES C CXX)

add_library(chromajxl SHARED "${CMAKE_CURRENT_LIST_DIR}/../chroma_jxl.c")
target_include_directories(chromajxl PRIVATE "${JXL_INCLUDE_DIR}")
if(WIN32)
    # Installed jxl headers default to dllimport. The static libs have no __imp_ symbols.
    target_compile_definitions(chromajxl PRIVATE JXL_STATIC_DEFINE)
endif()
target_link_libraries(
    chromajxl
    PRIVATE
        "${JXL_LIBRARY}"
        "${JXL_CMS_LIBRARY}"
        "${HWY_LIBRARY}"
        "${BROTLIENC_LIBRARY}"
        "${BROTLIDEC_LIBRARY}"
        "${BROTLICOMMON_LIBRARY}"
)
if(SKCMS_LIBRARY)
    target_link_libraries(chromajxl PRIVATE "${SKCMS_LIBRARY}")
endif()
set_target_properties(
    chromajxl
    PROPERTIES LINKER_LANGUAGE CXX OUTPUT_NAME chromajxl C_VISIBILITY_PRESET hidden
               CXX_VISIBILITY_PRESET hidden
)

if(NOT WIN32)
    find_package(Threads REQUIRED)
    target_link_libraries(chromajxl PRIVATE Threads::Threads)
endif()

if(APPLE)
    set_target_properties(chromajxl PROPERTIES INSTALL_NAME_DIR "@rpath" BUILD_RPATH "@loader_path")
endif()
